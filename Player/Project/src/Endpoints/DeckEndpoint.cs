using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Postgrest;
using Postgrest.Exceptions;
using Supabase.Storage.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Yog.Api.Models;

namespace Yog.Api.Endpoints
{
	public class DeckEndpoint
	{

		public DeckEndpoint(ILogger<DeckEndpoint> logger, ISupabaseClient supabaseClient)
		{
			_logger = logger;
			_supabaseClient = supabaseClient;
		}

		private const int MINDECKSIZE = 15;
		private const int MAXDECKSIZE = 20;

		private readonly ILogger<DeckEndpoint> _logger;
		private readonly ISupabaseClient _supabaseClient;


		[CloudCodeFunction("GetDeckServer")]
		public async Task<Deck> GetDeckServer(IExecutionContext context, string deckId, string playerId)
		{
			try
			{
				var deckGuid = Guid.Parse(deckId);
				var deck = await _supabaseClient.Connection.From<Deck>()
				.Select("id, name, Cards(name, cardType, race, attack, health, processorCost, memoryCost, " +
				"CardEffects(*))")
				.Where(x => x.PlayerId == playerId && x.Id == deckGuid)
				.Single();

				if (deck == null || deck.Cards == null) throw new Exception("Deck not found.");

				return deck;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Failed to get dec4k. Postgrest error. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (FormatException)
			{
				_logger.LogError("Attempted to get a deck without correct params.");
				throw new Exception("Bad request.");
			}
			catch (NullReferenceException)
			{
				_logger.LogError("Attempted to get a deck without correct params.");
				throw new Exception("Bad request.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supabase error. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get deck. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
		}

		[CloudCodeFunction("GetAIDeckServer")]
		public async Task<Deck> GetAIDeckServer()
		{
			try
			{
				var decks = await _supabaseClient.Connection.From<Deck>()
				.Select("id, name, Cards(name, cardType, race, attack, health, processorCost, memoryCost, " +
				"CardEffects(*))")
				.Where(x => x.PlayerId == "stFRgEyo4tEm0VJGiBdMUeUG9Pgu")
				.Get();
				
				var deck = decks.Models.FirstOrDefault();

				if (deck == null || deck.Cards == null) throw new Exception("Could not get AI deck.");

				return deck;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Failed to get dec4k. Postgrest error. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (FormatException)
			{
				_logger.LogError("Attempted to get a deck without correct params.");
				throw new Exception("Bad request.");
			}
			catch (NullReferenceException)
			{
				_logger.LogError("Attempted to get a deck without correct params.");
				throw new Exception("Bad request.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supabase error. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get deck. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
		}


		[CloudCodeFunction("GetAllPlayerDecks")]
		public async Task<List<Deck>> GetAllPlayerDecks(IExecutionContext context, IGameApiClient gameApiClient)
		{
			try
			{
				var decks = await _supabaseClient.Connection.From<Deck>()
				.Select("id, name, Cards!inner(name)")
				.Where(x => x.PlayerId == context.PlayerId)
				.Order(x => x.CreatedAt, Constants.Ordering.Ascending)
				.Get();

				foreach (var deck in decks.Models)
				{
					deck.Cards.Sort((x, y) => x.Name.CompareTo(y.Name));
					deck.CardNames = deck.Cards.Select(x => x.Name).ToList();
					deck.Cards = null;
				}

				if (decks.Models == null || decks.Models.Count == 0)
				{
					_logger.LogError("No decks found");
					throw new Exception("Decks not found.");
				}

				foreach (var card in decks.Models[0].CardNames)
				{
					_logger.LogInformation(card);
				}

				return decks.Models;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Reason);
				throw new Exception("Failed to get deck.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supabase error. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get deck. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
		}

		[CloudCodeFunction("CreateDeck")]
		public async Task CreateDeck(IExecutionContext context, string name, List<string> cardNames)
		{
			if (cardNames.Count < MINDECKSIZE || cardNames.Count > MAXDECKSIZE)
			{
				throw new Exception("Invalid number of cards.");
			}

			var deck = new Deck
			{
				Name = name,
				PlayerId = context.PlayerId,
			};

			try
			{
				var count = await _supabaseClient.Connection
				.From<Deck>()
				.Select(x => new object[] { x.Name })
				.Where(x => x.PlayerId == context.PlayerId)
				.Count(Postgrest.Constants.CountType.Exact);

				_logger.LogInformation("Creating deck with name {name}", name);

				var result = await _supabaseClient.Connection.From<Deck>().Insert(deck, new Postgrest.QueryOptions { Returning = Postgrest.QueryOptions.ReturnType.Representation });
				if (result.ResponseMessage != null && !result.ResponseMessage.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to create deck. Unsuccesful response received from database.");
					throw new Exception("Failed to create deck.");
				}

				var deckCards = cardNames.Select(x => new DeckCard() { Id = Guid.NewGuid(), DeckId = result.Model.Id, CardName = x }).ToList();
				await _supabaseClient.Connection.From<DeckCard>().Insert(deckCards);
				return;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}, {message}", e.Reason, e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supbase error. {error}", e.Message);
				throw new Exception("Failed to create deck.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to create deck. {error}", e.Message);
				throw new Exception("Failed to create deck.");
			}

		}

		[CloudCodeFunction("EditDeck")]
		public async Task EditDeck(IExecutionContext context, string deckId, List<string> cardNames)
		{
			if (cardNames.Count < MINDECKSIZE || cardNames.Count > MAXDECKSIZE)
			{
				throw new Exception("Invalid number of cards.");
			}

			try
			{
				var deckGuid = Guid.Parse(deckId);

				//TODO: back these up first
				var deckCards = cardNames.Select(x => new DeckCard() { Id = Guid.NewGuid(), DeckId = deckGuid, CardName = x }).ToList();

				await _supabaseClient.Connection.From<DeckCard>()
				.Where(x => x.DeckId == deckGuid)
				.Delete();

				await _supabaseClient.Connection.From<DeckCard>()
				.Insert(deckCards);

				await _supabaseClient.Connection.From<Deck>()
				.Where(x => x.Id == deckGuid)
				.Set(x => x.EditedAt, DateTime.UtcNow)
				.Update();
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supabase error. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get deck. {error}", e.Message);
				throw new Exception("Failed to get deck.");
			}
		}

		[CloudCodeFunction("CreateStarterDecksServer")]
		public async Task CreateStarterDecks(List<StarterDeck> decks)
		{
			try
			{

				List<StarterDeck_Card> cards = new();
				foreach (var deck in decks)
				{

					var cardIds = deck.CardNames;
					deck.CardNames = null;

					if (cardIds == null || cardIds.Count == 0)
					{
						_logger.LogError("Deck has no cards.");
						throw new Exception("Deck has no cards.");
					}
					await _supabaseClient.Connection.From<StarterDeck>().Upsert(decks);
					// await _supabaseClient.Connection.From<StarterDeck_Card>()
					// 	.Where(x => x.DeckId != Guid.Empty)
					// 	.Delete();

					foreach (var cardId in cardIds)
					{
						cards.Add(new StarterDeck_Card { Id = Guid.NewGuid(), DeckId = deck.Id, CardName = cardId });
					}
				}

				await _supabaseClient.Connection.From<StarterDeck_Card>().Upsert(cards);
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to create starter decks. {error}", e.Message);
				_logger.LogTrace(e, "Failed to create starter decks.");
				throw new Exception("Failed to create starter decks.");
			}
		}

		[CloudCodeFunction("DeleteDeck")]
		public async Task DeleteDeckById(IExecutionContext context, string deckId)
		{
			try
			{
				var deckGuid = Guid.Parse(deckId);

				await _supabaseClient.Connection.From<Deck>()
				.Where(x => x.Id == deckGuid && x.PlayerId == context.PlayerId)
				.Delete();
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Message);
				throw new Exception("Failed to delete deck.");
			}
			catch (FormatException)
			{
				throw new Exception("Bad request.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supabase error. {error}", e.Message);
				throw new Exception("Failed to delete deck.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get deck. {error}", e.Message);
				throw new Exception("Failed to delete deck.");
			}
		}
	}
}