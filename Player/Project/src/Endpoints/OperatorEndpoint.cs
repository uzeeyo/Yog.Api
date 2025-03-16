using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Postgrest.Exceptions;
using Supabase.Storage.Exceptions;
using Unity.Services.CloudCode.Core;
using Yog.Api.Models;

namespace Yog.Api.Endpoints
{
	public class OperatorEndpoint
	{
		public OperatorEndpoint(ILogger<OperatorEndpoint> logger, ISupabaseClient supabaseClient)
		{
			_logger = logger;
			_supabaseClient = supabaseClient;
		}

		private readonly ILogger<OperatorEndpoint> _logger;
		private readonly ISupabaseClient _supabaseClient;

		[CloudCodeFunction("GetOperator")]
		public async Task<Operator> GetOperatorData(System.Guid id, IExecutionContext context)
		{
			try
			{
				var opData = await _supabaseClient.Connection.From<Operator>()
				.Select(x => new object[] { x.Id, x.Name, x.Health, x.Memory, x.Shields, x.ShieldRegen })
				.Where(x => x.Id == id)
				.Single();

				return opData;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Reason);
				throw new Exception("Failed to get operator.");
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
		
		[CloudCodeFunction("GetAIOperator")]
		public async Task<Operator> GetAIOperatorData()
		{
			try
			{
				var opData = await _supabaseClient.Connection.From<Operator>()
				.Select(x => new object[] { x.Id, x.Name, x.Health, x.Memory, x.Shields, x.ShieldRegen })
				.Where(x => x.Name == "LUNA")
				.Single();
				
				if (opData == null)
				{
					throw new Exception("Failed to get operator.");
				}

				return opData;
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Reason);
				throw new Exception("Failed to get operator.");
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
		
		[CloudCodeFunction("GetAllOperators")]
		public async Task<List<Operator>> GetAllOperatorData()
		{
			try
			{
				var opData = await _supabaseClient.Connection.From<Operator>()
				.Select(x => new object[] { x.Id, x.Name, x.Description, x.Health, x.Memory, x.Shields, x.ShieldRegen })
				.Get();

				return opData.Models.ToList();
			}
			catch (PostgrestException e)
			{
				_logger.LogError("Postgrest error. {error}", e.Reason);
				throw new Exception("Failed to get operator.");
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