
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
		[PrimaryKey("id")]
		public Guid Id { get; set;}
		
		[Column("name")]
		public string Name { get; set; } = "";
		
		[Column("cardType")]
		public int CardType { get; set; }
		
		[Column("attack"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? Attack { get; set; }
		
		[Column("health"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? Health { get; set; }
		
		[Column("elementType")]
		public int ElementType { get; set; }
		
		[Column("raceType")]
		public int RaceType { get; set; }
		
		[Column("description")]
		public string Description { get; set; } = "";
		
		[Column("imagePath")]
		public string ImagePath { get; set; } = "";
		
		[Column("processorCost"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? ProcessorCost { get; set; }
		
		[Column("memoryCost"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? MemoryCost { get; set; }
		
		[Reference(typeof(CardEffect), false), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<CardEffect>? Effects { get; set; }
		
		[Column("createdAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? CreatedAt { get; set; }
		
		[Column("archivedAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? ArchivedAt { get; set; }
		
		[Column("editedAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? EditedAt { get; set; }	
	}
}