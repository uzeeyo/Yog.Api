using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
	[Table("Decks")]
	public class Deck : BaseModel
	{
		[PrimaryKey("id")]
		public Guid Id { get; set; }

		[Column("name")]
		public string Name { get; set; }

		[Column("playerId"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? PlayerId { get; set; }

		[Reference(typeof(Card), false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<Card>? Cards { get; set; }

		[Column("createdAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? CreatedAt { get; set; }

		[Column("editedAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? EditedAt { get; set; }

		[Column("archivedAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? ArchivedAt { get; set; }
		
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> CardNames { get; set; }
	}
}