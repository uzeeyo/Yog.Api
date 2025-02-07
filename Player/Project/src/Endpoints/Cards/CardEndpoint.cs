using System;
using System.Collections.Generic;
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
		
		[CloudCodeFunction("CreateCardServer")]
		public async Task CreateCard(IExecutionContext context, Card card)
		{
			try 
			{
				card.CreatedAt = DateTime.UtcNow;
				var insertedCard = await _supabaseClient.Connection.From<Card>().Insert(card, new QueryOptions { Returning = QueryOptions.ReturnType.Representation });
				_logger.LogInformation("Created card: {name}.", card.Name);
				await _supabaseClient.Connection.From<PlayerCard>().Insert(new PlayerCard { PlayerId = "stFRgEyo4tEm0VJGiBdMUeUG9Pgu", CardId = insertedCard.Model.Id, Count = 1 });
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Reason);
				throw new Exception("Failed to create card.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supbase error. {error}", e.Message);
				throw new Exception("Failed to create card.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to create card. {error}", e.Message);
				throw new Exception("Failed to create card.");
			}
		}
		
		[CloudCodeFunction("EditCardServer")]
		public async Task EditCard(IExecutionContext context, Card card)
		{
			try 
			{
				
				var oldCard = await _supabaseClient.Connection.From<Card>()
				.Where(x => x.Id == card.Id)
				.Single();
				
				if (oldCard == null) 
				{
					throw new Exception("Card not found.");
				} 
				
				await card.Update<Card>();
				
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Reason);
				throw new Exception("Failed to edit card.");
			}
			catch (SupabaseStorageException e)
			{
				_logger.LogError("Supbase error. {error}", e.Message);
				throw new Exception("Failed to edit card.");
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to create card. {error}", e.Message);
				throw new Exception("Failed to edit card.");
			}
		}
		
		[CloudCodeFunction("GetUnlockedCards")]
		public async Task<List<Card>> GetUnlockedCards(IExecutionContext context)
		{
			try 
			{
				var cards = await _supabaseClient.Connection.From<Card>()
				.Select("id, name, attack, health, processorCost, memoryCost, description, imagePath, elementType, cardType, raceType, Player_Cards!inner(*)")
				.Filter("Player_Cards.playerId", Constants.Operator.Equals, context.PlayerId)
				.Order(x => x.Name, Constants.Ordering.Descending)
				.Get();
				
				if (cards.Models.Count == 0) 
				{
					_logger.LogError("No unlocked cards found for player {id}.", context.PlayerId);
					throw new Exception("No cards found.");
				}
				
				return cards.Models;
			}
			catch(Exception e) 
			{
				_logger.LogError("Failed to get unlocked cards. {error}", e.Message);
				throw new Exception("Failed to get unlocked cards.");
			}
		}
		
		[CloudCodeFunction("GetAllCardsServer")]
		public async Task<List<Card>> GetAllCardsServer()
		{
			try 
			{
				var cards = await _supabaseClient.Connection.From<Card>()
				.Select("id, name, attack, health, processorCost, memoryCost, description, imagePath, cardType, elementType, raceType")
				.Order(x => x.Name, Constants.Ordering.Descending)
				.Get();
				
				if (cards.Models.Count == 0) 
				{
					_logger.LogError("No cards found.");
					throw new Exception("No cards found.");
				}
				
				return cards.Models;
			}
			catch(Exception e) 
			{
				_logger.LogError("Failed to get all cards. {error}", e.Message);
				throw new Exception("Failed to get all cards.");
			}
		}
		
		[CloudCodeFunction("DeleteCardServer")]
		public async Task DeleteCardServer(IExecutionContext context, string cardId)
		{
			try 
			{
				var cardGuid = Guid.Parse(cardId);
				
				var card = await _supabaseClient.Connection.From<Card>()
				.Where(x => x.Id == cardGuid)
				.Single();
				
				if (card == null) 
				{
					throw new Exception("Card not found.");
				}
				
				card.ArchivedAt = DateTime.UtcNow;
				
				await card.Update<Card>();
			}
			catch(Exception e) 
			{
				_logger.LogError("Failed to delete card. {error}", e.Message);
				throw new Exception("Failed to delete card.");
			}
		}
	}
}