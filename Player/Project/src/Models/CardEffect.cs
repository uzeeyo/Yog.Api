using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api
{
	[Table("CardEffects")]
	public class CardEffect : BaseModel
	{
		[PrimaryKey("id"), Column("id")]
		public Guid Id { get; set; }

		[Column("effectType")]
		public int EffectType { get; set; }

		[Column("activationType")]
		public int ActivationType { get; set; }

		[Column("turnPhase")]
		public int TurnPhase { get; set; }

		[Column("targetSide")]
		public int TargetSide { get; set; }

		[Column("targetType"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? TargetType { get; set; }

		[Column("selectionType"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? SelectionType { get; set; }

		[Column("conditionType"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int ConditionType { get; set; }

		[Column("comparisonType"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int ComparisonType { get; private set; }

		[Column("integerCondition"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int IntegerCondition { get; private set; }

		[Column("raceCondition"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int RaceCondition { get; private set; }

		[Column("turnsActive"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? TurnsActive { get; set; }

		[Column("amount1", NullValueHandling.Ignore), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? Amount1 { get; set; }

		[Column("amount2", NullValueHandling.Ignore), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int? Amount2 { get; set; }

		[Column("inclusionType")]
		public int InclusionType { get; set; }

		[Column("uniquePassiveActivation")]
		public bool UniquePassiveActivation { get; set; }
		
		[Column("passiveActivationTurn")]
		public int PassiveActivationTurn { get; set; }
	}
}