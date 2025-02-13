using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Supabase;
using Unity.Services.CloudCode.Apis;
using Yog.Api;

namespace Yog.Database 
{
	public class SupabaseClient : ISupabaseClient
	{
		public SupabaseClient() 
		{
			var options = new SupabaseOptions 
			{
				AutoConnectRealtime = true,
				AutoRefreshToken = true,
			};
			
			Connection = new Client(Secret.SUPABASE_URL, Secret.SUPABASE_KEY, options);
		}
		
		
		
		public Client Connection { get; }
		
		public static ISupabaseClient Init() => new SupabaseClient();
	}
}