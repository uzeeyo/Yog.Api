using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Postgrest;
using Unity.Services.CloudCode.Core;
using Yog.Api.Models;
using Yog.Database;

namespace Yog.Api 
{
	public class GetCard 
	{
		public GetCard(ILogger<GetCard> logger, ISupabaseClient supabaseClient) 
		{
			_logger = logger;
			_supabaseClient = supabaseClient;
		}
		
		private readonly ILogger _logger;
		private readonly ISupabaseClient _supabaseClient;
		
		[CloudCodeFunction("GetCard")]
		public async Task<Card> Get(IExecutionContext context, string cardId)
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
				
				return card;
			}
			catch(Exception e) 
			{
				_logger.LogError("Failed to get card. {error}", e.Message);
				throw new Exception("Failed to get card.");
			}
		}
	}
}