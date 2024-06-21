using System;
using System.Data.Common;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api 
{
	[Table("CardEffects")]
	public class CardEffect : BaseModel
	{
		[PrimaryKey("id")]
		public Guid Id { get; set; }
		
		[Column("effectType")]
		public int EffectType { get; set; }
		
		[Column("turnPhase")]
		public int TurnPhase { get; set; }
		
		[Column("targetSide")]
		public int TargetSide { get; set; }
		
		[Column("targetType"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? TargetType { get; set; }
		
		[Column("selectionType"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? SelectionType { get; set; }
		
		[Column("condition"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string? Condition { get; set; }
		
		[Column("amount1"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? Amount1 { get; set; }
		
		[Column("amount2"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? Amount2 { get; set; }
		
		[Column("createdAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? CreatedAt { get; set; }
		
		[Column("editedAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? EditedAt { get; set; }
	}
}