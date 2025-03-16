using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Postgrest;
using Postgrest.Exceptions;
using Unity.Services.CloudCode.Core;
using Yog.Api.Models;
using Yog.Database;

namespace Yog.Api.Endpoints
{
	public class PackEndpoint
	{
		public PackEndpoint(ILogger<PackEndpoint> logger)
		{
			_logger = logger;
		}

		private readonly ILogger _logger;

		[CloudCodeFunction("OpenPack")]
		public async Task<List<Card>> OpenPack(Guid packId, IExecutionContext context, ISupabaseClient supabase)
		{
			try
			{
				var pack = await supabase.Connection.From<Pack>()
					.Where(x => x.Id == packId)
					.Select("cost")
					.Single();

				if (pack == null)
				{
					_logger.LogWarning($"Player {context.PlayerId} tried to open a pack that doesn't exist.");
					throw new Exception("Pack does not exist.");
				}

				var player = await supabase.Connection.From<Player>()
					.Where(x => x.Id == context.PlayerId)
					.Select(x => new object[] { x.Shards })
					.Single();

				if (player == null)
				{
					_logger.LogWarning($"Tried to open a pack as a player that doesn't exist. Player : {context.PlayerId}");
					throw new Exception("Player does not exist.");
				}

				if (pack.Cost > player.Shards)
				{
					_logger.LogWarning("Tried to unlock pack without enough shards. Player: {0}", context.PlayerId);
					throw new Exception();
				}

				var allCards = await supabase.Connection.From<Card>()
				.Where(x => x.PackId == packId)
				.Select(x => new object[] { x.Name })
				.Get();

				var cards = allCards.Models.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

				await supabase.Connection.From<Player>()
				.Where(x => x.Id == context.PlayerId)
				.Set(x => x.Shards, player.Shards - pack.Cost)
				.Update();

				var unlockedCards = new List<PlayerCard>();
				foreach (var card in cards)
				{
					unlockedCards.Add(new PlayerCard { PlayerId = context.PlayerId, CardName = card.Name, Count = 1 });
				}

				var queryOptions = new QueryOptions { Returning = QueryOptions.ReturnType.Minimal, DuplicateResolution = QueryOptions.DuplicateResolutionType.IgnoreDuplicates };
				await supabase.Connection.From<PlayerCard>()
					.Upsert(unlockedCards, queryOptions);

				return cards;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("PostgrestExcept: {0}", e.Message);
				throw new Exception();
			}
			catch (Exception e)
			{
				_logger.LogError("Failed to get open pack: {0}", e.Message);
				throw new Exception();
			}
		}

		[CloudCodeFunction("EditPacks")]
		public async Task EditPacks(List<Pack> packs, ISupabaseClient _supabaseClient)
		{
			try
			{
				await _supabaseClient.Connection.From<Card>()
					.Filter(x => x.PackId, Constants.Operator.NotEqual, Guid.NewGuid().ToString())
					.Set(x => x.PackId, null)
					.Update();
					
				_logger.LogInformation($"Editing packs: {packs.Count} packs");
				
				foreach (var pack in packs)
				{
					_logger.LogInformation($"Editing pack: {pack.Id}");
					List<object> cardNames = pack.CardNames.Select(x => (object)x).ToList();
					pack.CardNames = null;
					pack.CreatedAt = DateTime.UtcNow;
					await _supabaseClient.Connection.From<Pack>()
						.Upsert(pack);
					var cards = await _supabaseClient.Connection.From<Card>()
						.Filter(x => x.Name, Constants.Operator.In, cardNames)
						.Set(x => x.PackId, pack.Id)
						.Update();
				}
			}
			catch (Exception e)
			{
				_logger.LogTrace(e, "Failed to edit packs");
				throw new Exception();
			}
		}
	}
}