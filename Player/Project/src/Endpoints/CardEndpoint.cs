using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Postgrest;
using Postgrest.Exceptions;
using Supabase.Storage.Exceptions;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Yog.Api.Models;
using Yog.Database;

namespace Yog.Api
{
	public class CardEndpoint
	{
		public CardEndpoint(ILogger<CardEndpoint> logger, IGameApiClient gameApiClient, ISupabaseClient supabaseClient)
		{
			_logger = logger;
			_supabaseClient = supabaseClient;
		}

		private readonly ILogger _logger;
		private readonly ISupabaseClient _supabaseClient;

		[CloudCodeFunction("GetUnlockedCards")]
		public async Task<List<string>> GetUnlockedCards(IExecutionContext context)
		{
			try
			{
				var cards = await _supabaseClient.Connection.From<Card>()
				.Select("name, Player_Cards!inner(*)")
				.Filter("Player_Cards.playerId", Constants.Operator.Equals, context.PlayerId)
				.Order(x => x.Name, Constants.Ordering.Descending)
				.Get();


				if (cards.Models.Count == 0)
				{
					_logger.LogError("No unlocked cards found for player {id}.", context.PlayerId);
					throw new Exception("No cards found.");
				}
				var cardNames = cards.Models.Select(x => x.Name).ToList();

				return cardNames;
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get unlocked cards. {error}", e.Message);
				throw new Exception("Failed to get unlocked cards.");
			}
		}

		[CloudCodeFunction("EditCardsServer")]
		public async Task EditCardsServer(List<Card> cards)
		{
			if (cards == null)
			{
				_logger.LogError("No cards received");
				throw new Exception();
			}
			try
			{
				await _supabaseClient.Connection.Rpc("deleteallcardeffects", null);
				await _supabaseClient.Connection.From<Card>().Upsert(cards);
				List<CardEffect> effects = new();
				List<CardCardEffect> effectLinks = new();
				HashSet<Guid> ids = new();
				foreach (var card in cards)
				{
					foreach (var effect in card.Effects)
					{
						if (effect == null || effect.Id == default)
						{
							throw new Exception("Tried adding 0 id");
						}
						if (!ids.Contains(effect.Id))
						{
							effects.Add(effect);
						}
						ids.Add(effect.Id);
						effectLinks.Add(new CardCardEffect()
						{
							CardId = card.Id,
							EffectId = effect.Id
						});
					}
				}


				_logger.LogInformation(effects.Count.ToString() + "effects");
				if (effects.Count > 0)
				{
					await _supabaseClient.Connection.From<CardEffect>().Upsert(effects);
					await _supabaseClient.Connection.From<CardCardEffect>().Upsert(effectLinks);
				}
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Message);
				throw new Exception("Failed to edit cards.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supbase error. {error}", e.Message);
				throw new Exception("Failed to edit cards.");
			}
			catch (Exception e)
			{
				_logger.LogTrace(e, "Failed to edit cards");
				throw new Exception("Failed to edit cards.");
			}
		}

		[CloudCodeFunction("DeleteCardServer")]
		public async Task DeleteCardServer(IExecutionContext context, string cardName)
		{
			try
			{
				var card = await _supabaseClient.Connection.From<Card>()
				.Where(x => x.Name == cardName)
				.Single();

				if (card == null)
				{
					throw new Exception("Card not found.");
				}

				await card.Update<Card>();
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to delete card. {error}", e.Message);
				throw new Exception("Failed to delete card.");
			}
		}
	}
}