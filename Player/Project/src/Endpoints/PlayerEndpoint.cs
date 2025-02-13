
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Postgrest.Exceptions;
using Unity.Services.CloudCode.Core;
using Postgrest;
using Yog.Api.Models;
using System.Linq;
using Websocket.Client;

namespace Yog.Api.Endpoints
{
	public class PlayerEndpoint
	{
		public PlayerEndpoint(ILogger<PlayerEndpoint> logger, ISupabaseClient supabaseClient)
		{
			_logger = logger;
			_supabaseClient = supabaseClient;

		}

		const string PLAYER_DATA_QUERY = "id, xp, shards, gold, selectedCoreId, Decks(id, name, Cards(name))";

		private readonly ILogger<PlayerEndpoint> _logger;
		private readonly ISupabaseClient _supabaseClient;


		[CloudCodeFunction("GetPlayerDetails")]
		public async Task<Player> GetPlayerDetails(string steamAuthTicket, IExecutionContext context)
		{
			try
			{
				if (string.IsNullOrEmpty(context.PlayerId))
				{
					throw new Exception("PlayerId is required.");
				}
				


				var player = await _supabaseClient.Connection.From<Player>()
				.Select(PLAYER_DATA_QUERY)
				.Where(x => x.Id == context.PlayerId)
				.Single();

				foreach (var deck in player.Decks)
				{
					deck.CardNames = deck.Cards.Select(x => x.Name).ToList();
					deck.Cards = null;
				}

				var packs = await _supabaseClient.Connection.From<Pack>()
				.Select(x => new object[] { x.Id, x.Name, x.Cost, x.PackType })
				.Get();

				player ??= await SetupNewPlayer(context.PlayerId, steamAuthTicket);



				player.SteamId = null;
				player.CreatedAt = null;
				player.Packs = packs.Models;
				return player;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest failed to get player. {error}", e.Message);
				throw;
			}
			catch (NullReferenceException e)
			{
				_logger.LogError(e.Message);
				throw;
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get player. {error}", e.Message);
				throw new Exception("Failed to get player.");
			}
		}

		private async Task<Player> SetupNewPlayer(string playerId, string steamAuthTicket)
		{
			try
			{
				var url = $"https://partner.steam-api.com/ISteamUserAuth/AuthenticateUserTicket/v1/?key={Secret.STEAM_API_KEY}&appId={Secret.STEAM_APP_ID}&ticket={steamAuthTicket}&identity=xalos";
				var steamRes = await HttpRequest.Get<SteamResponse>(url);
								
				var player = new Player
				{
					Id = playerId,
					SteamId = steamRes.Response.Params.SteamID,
					CreatedAt = DateTime.UtcNow,
					Xp = 0,
					Shards = 100,
					Gold = 0,
					Decks = null
				};

				var insertedPlayer = await _supabaseClient.Connection.From<Player>()
						.Insert(player
						, new QueryOptions
						{
							Returning = QueryOptions.ReturnType.Representation
						});

				if (insertedPlayer == null || insertedPlayer.Model == null)
				{
					throw new Exception("Player not inserted.");
				}

				//Create Starter Deck
				var deck = await _supabaseClient.Connection.From<Deck>().Insert(new Deck
				{
					Id = Guid.NewGuid(),
					Name = "Starter Deck",
					PlayerId = playerId,
					CreatedAt = DateTime.UtcNow
				}, new QueryOptions
				{
					Returning = QueryOptions.ReturnType.Representation
				});

				if (deck == null || deck.Model == null) throw new Exception();


				var allCards = await _supabaseClient.Connection.From<Card>()
				.Select(x => new object[] { x.Name })
				.Get();

				var randomCards = allCards.Models.Concat(allCards.Models).OrderBy(x => Guid.NewGuid()).Take(20);
				var unlockedCards = randomCards.Select(x => new DeckCard() { DeckId = deck.Model.Id, CardName = x.Name });
				if (unlockedCards is null) throw new Exception();

				await _supabaseClient.Connection.From<DeckCard>().Insert(unlockedCards.ToList());

				var newPlayer = await _supabaseClient.Connection.From<Player>()
				.Select(PLAYER_DATA_QUERY)
				.Where(x => x.Id == playerId)
				.Single();

				return newPlayer;
			}
			catch (NullReferenceException e)
			{
				_logger.LogError("Failed to create new player. {error}", e.Message);
				throw new Exception("Failed to create new player.");
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest failed to create new player. {error}", e.Message);
				throw new Exception("Failed to create new player.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to create new player. {error}", e.Message);
				throw new Exception("Failed to create new player.");
			}
		}

		[CloudCodeFunction("VerifyPlayerServer")]
		public async Task VerifyPlayer(string playerId, string steamId)
		{
			try
			{
				var player = await _supabaseClient.Connection.From<Player>()
				.Where(x => x.Id == playerId)
				.Single();

				if (player == null)
				{
					throw new Exception("Player not found");
				}
				if (player.SteamId != steamId)
				{
					throw new Exception($"Steam Id does not match actual {player.SteamId}");
				}

			}
			catch (Exception e)
			{
				_logger.LogError(e.Message);
				throw new Exception("Failed to verify");
			}

		}

		[CloudCodeFunction("UpdatePlayerXpServer")]
		public async Task UpdatePlayerXp(string playerId, int xp)
		{
			try
			{
				var player = await _supabaseClient.Connection.From<Player>()
				.Where(x => x.Id == playerId)
				.Single();

				if (player == null)
				{
					throw new Exception("Player not found.");
				}

				var newXp = player.Xp + xp;
				_logger.LogInformation("Updating player xp. PlayerId: {playerId}, OldXp: {oldXp}, NewXp: {newXp}", playerId, player.Xp, newXp);

				await _supabaseClient.Connection.From<Player>()
				.Where(x => x.Id == playerId)
				.Set(x => x.Xp, newXp)
				.Update();


			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Reason);
				throw new Exception("Failed to update player xp.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to update player xp. {error}", e.Message);
				throw new Exception("Failed to update player xp.");
			}
		}
	}
}