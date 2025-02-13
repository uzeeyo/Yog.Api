
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
	[Table("Cards")]
	public class Card : BaseModel
	{
		[PrimaryKey("id"), Column("id")]
		public Guid Id { get; set; }
		
		[Column("name")]
		public string Name { get; set; } = "";

		[Column("packId", NullValueHandling.Ignore), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Guid? PackId { get; set; }

		[Column("race")]
		public int Race { get; set; }
		
		[Column("cardType")]
		public int CardType { get; set; }
		
		[Column("attack"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int Attack { get; set; }
		
		[Column("health"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int Health { get; set; }
		
		[Column("processorCost"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int ProcessorCost { get; set; }
		
		[Column("memoryCost"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int MemoryCost { get; set; }
		
		[Reference(typeof(CardEffect), includeInQuery: false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<CardEffect>? Effects { get; set; }
	}
}