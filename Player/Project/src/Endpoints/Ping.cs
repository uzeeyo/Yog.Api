using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;
using Yog.Database;

namespace Yog.Api.Endpoints 
{
	public class Ping 
	{
		public Ping(ILogger<Ping> logger)
		{
			_logger = logger;
		}
		
		private readonly ILogger<Ping> _logger;
		
		[CloudCodeFunction("Ping")]
		public Task PingServer() 
		{
			return Task.CompletedTask;
		}
	}
}