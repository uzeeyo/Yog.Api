using Microsoft.Extensions.DependencyInjection;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Yog.Database;

namespace Yog.Api 
{
	public class ModuleConfig : ICloudCodeSetup
	{
		public void Setup(ICloudCodeConfig config)
		{		
			config.Dependencies.AddSingleton(GameApiClient.Create());
			config.Dependencies.AddSingleton(SupabaseClient.Init());	
		}
	}
}