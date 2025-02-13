namespace Yog.Api 
{
	class SteamResponse
		{
			public ResponseData Response { get; set; }
		}

		class ResponseData
		{
			public ParamsData Params { get; set; }
		}

		class ParamsData
		{
			public string Result { get; set; }
			public string SteamID { get; set; }
			public string OwnerSteamID { get; set; }
			public bool VACBanned { get; set; }
			public bool PublisherBanned { get; set; }
		}
}