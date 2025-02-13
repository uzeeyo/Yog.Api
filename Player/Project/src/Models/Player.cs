using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
	[Table("Players")]
	public class Player : BaseModel
	{
		[PrimaryKey("id"), Column("id")]
		public string Id { get; set; }

		[Column("steamId"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? SteamId { get; set; }

		[Column("createdAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? CreatedAt { get; set; }

		[Column("xp")]
		public int Xp { get; set; }

		[Column("shards")]
		public int Shards { get; set; }

		[Column("gold")]
		public int Gold { get; set; }

		[Column("Decks", NullValueHandling.Ignore, true, true)]
		public List<Deck>? Decks { get; set; }

		[Reference(typeof(Pack), false)]
		public List<Pack>? Packs { get; set; }

	}

}
