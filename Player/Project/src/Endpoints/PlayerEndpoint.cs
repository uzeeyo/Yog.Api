
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Postgrest.Exceptions;
using Unity.Services.CloudCode.Core;
using Postgrest;
using Yog.Api.Models;
using System.Linq;
using Websocket.Client;
using System.Collections.Generic;

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
				var player = await _supabaseClient.Connection.From<Player>()
				.Select(PLAYER_DATA_QUERY)
				.Where(x => x.Id == context.PlayerId)
				.Single();

				string verifiedSteamId = string.Empty;
				if (!string.IsNullOrEmpty(steamAuthTicket))
				{

					var url = $"https://partner.steam-api.com/ISteamUserAuth/AuthenticateUserTicket/v1/?key={Secret.STEAM_API_KEY}&appId={Secret.STEAM_APP_ID}&ticket={steamAuthTicket}&identity=xalos";
					var steamRes = await HttpRequest.Get<SteamResponse>(url);
					
					verifiedSteamId = steamRes.Response.Params.SteamID;
				}

				player ??= await SetupNewPlayer(context.PlayerId, verifiedSteamId);
				if (player == null)
				{
					throw new Exception("Player not found.");
				}

				if (player.SteamId != null && string.IsNullOrEmpty(verifiedSteamId)) throw new Exception("Invalid Steam ID");
				if (player.SteamId != null && player.SteamId != verifiedSteamId) throw new Exception("Failed to verify credentials");
				if (player.SteamId == null && !string.IsNullOrEmpty(verifiedSteamId)) 
				{
					await _supabaseClient.Connection.From<Player>()
						.Where(x => x.Id == context.PlayerId)
						.Set(x => x.SteamId, verifiedSteamId)
						.Update();
				}


				foreach (var deck in player.Decks)
				{
					deck.CardNames = deck.Cards.Select(x => x.Name).ToList();
					deck.Cards = null;
				}

				await _supabaseClient.Connection.From<Player>()
					.Where(x => x.Id == context.PlayerId)
					.Set(x => x.LastLogin, DateTime.UtcNow)
					.Update();

				player.SteamId = null;
				player.CreatedAt = null;
				player.LastLogin = null;

				return player;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest failed to get player. {error}", e.Message);
				throw new Exception("Failed to get player data");
			}
			catch (NullReferenceException e)
			{
				_logger.LogError(e.Message);
				throw new Exception("Failed to get player data");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get player. {error}", e.Message);
				throw new Exception("Failed to get player data");
			}
		}

		private async Task<Player> SetupNewPlayer(string playerId, string steamId)
		{
			try
			{
				var player = new Player
				{
					Id = playerId,
					SteamId = string.IsNullOrEmpty(steamId) ? null : steamId,
					CreatedAt = DateTime.UtcNow,
					LastLogin = DateTime.UtcNow,
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

				var starterDecks = await _supabaseClient.Connection.From<StarterDeck>().Get();

				var deckCards = new List<DeckCard>();
				foreach (var starterDeck in starterDecks.Models)
				{
					if (starterDeck.Cards == null || starterDeck.Cards.Count == 0)
					{
						_logger.LogError("Starter deck {deckName} has no cards", starterDeck.Name);
						throw new Exception("Starter deck has no cards");
					}

					var deck = await _supabaseClient.Connection.From<Deck>().Insert(new Deck
					{
						Id = Guid.NewGuid(),
						Name = starterDeck.Name,
						PlayerId = playerId,
						CreatedAt = DateTime.UtcNow
					}, new QueryOptions
					{
						Returning = QueryOptions.ReturnType.Representation
					});
					if (deck == null || deck.Model == null) throw new Exception();

					deckCards.AddRange(starterDeck.Cards.Select(x => new DeckCard() { DeckId = deck.Model.Id, CardName = x.CardName }).ToList());
				}
				await _supabaseClient.Connection.From<DeckCard>().Insert(deckCards);

				var unlockedCards = new HashSet<PlayerCard>();
				foreach (var deckCard in deckCards)
				{
					if (unlockedCards.Any(x => x.CardName == deckCard.CardName))
					{
						var card = unlockedCards.First(x => x.CardName == deckCard.CardName);
						card.Count++;
					}
					else
					{
						unlockedCards.Add(new PlayerCard { PlayerId = playerId, CardName = deckCard.CardName, Count = 1 });
					}
				}

				await _supabaseClient.Connection.From<PlayerCard>().Insert(unlockedCards);

				var newPlayer = await _supabaseClient.Connection.From<Player>()
				.Select(PLAYER_DATA_QUERY)
				.Where(x => x.Id == playerId)
				.Single();

				return newPlayer;
			}
			catch (NullReferenceException e)
			{
				_logger.LogTrace(e, "Failed to create new player.");
				throw new Exception("Failed to create new player.");
			}
			catch (PostgrestException e)
			{
				_logger.LogTrace(e, "Postgrest failed to create new player. {error}", e.Message);
				throw new Exception("Failed to create new player.");
			}
			catch (Exception e)
			{
				_logger.LogTrace(e, "Failed to create new player. {error}", e.Message);
				throw new Exception("Failed to create new player.");
			}
		}

		[CloudCodeFunction("GetPlayerDetailsAnon")]
		public async Task<Player> GetPlayerDetailsAnon()
		{
			try
			{
				var player = new Player();
				var decks = await _supabaseClient.Connection.From<Deck>()
					.Where(x => x.PlayerId == "anon")
					.Get();

				if (decks == null || decks.Models == null || decks.Models.Count == 0)
				{
					throw new Exception("No decks found for anon player.");
				}
				player.Decks = decks.Models;
				foreach (var deck in player.Decks)
				{
					deck.CardNames = deck.Cards.Select(x => x.Name).ToList();
					deck.Cards = null;
				}
				player.Id = "xalos";

				return player;
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get player. {error}", e.Message);
				throw new Exception("Failed to get player.");
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

				if (string.IsNullOrEmpty(steamId) && !string.IsNullOrEmpty(player.SteamId))
				{
					throw new Exception("Requested anonymous signin for non-anonymous player.");
				}

				if (!string.IsNullOrEmpty(steamId) && steamId != player.SteamId)
				{
					throw new Exception("Steam ID does not match player ID.");
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