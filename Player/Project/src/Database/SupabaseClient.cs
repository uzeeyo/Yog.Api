using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Supabase;
using Unity.Services.CloudCode.Apis;

namespace Yog.Database 
{
	public class SupabaseClient : ISupabaseClient
	{
		public SupabaseClient() 
		{
			//this needs to be hardcoded until Unity adds support for environment variables
			var url = "";
			var key = "";
			var options = new SupabaseOptions 
			{
				AutoConnectRealtime = true,
				AutoRefreshToken = true,
			};
			
			Connection = new Client(url, key, options);
		}
		
		
		
		public Client Connection { get; }
		
		public static ISupabaseClient Init() => new SupabaseClient();
	}
}