using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Postgrest;
using Postgrest.Exceptions;
using Unity.Services.CloudCode.Core;
using Yog.Api.Models;

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
				.Select(x => new object[] { x.Id, x.Name, x.CardType, x.Attack, x.Health, x.ProcessorCost, x.MemoryCost, x.Description, x.RaceType, x.ImagePath })
				.Get();

				var cards = allCards.Models.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

				await supabase.Connection.From<Player>()
				.Where(x => x.Id == context.PlayerId)
				.Set(x => x.Shards, player.Shards - pack.Cost)
				.Update();

				var openedPack = await supabase.Connection.From<PlayerPack>()
				.Where(x => x.PlayerId == context.PlayerId && x.PackId == packId)
				.Limit(1)
				.Single();

				await openedPack.Delete<PlayerPack>();

				var unlockedCards = new List<PlayerCard>();
				foreach (var card in cards)
				{
					unlockedCards.Add(new PlayerCard { PlayerId = context.PlayerId, CardId = card.Id, Count = 1 });
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
	}
}