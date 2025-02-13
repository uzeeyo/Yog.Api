using Postgrest.Models;
using Postgrest.Attributes;
using System;

namespace Yog.Api.Models
{
	[Table("Card_CardEffects")]
	public class CardCardEffect : BaseModel
	{
		[PrimaryKey, Column("cardId")]
		public Guid CardId { get; set; }

		[PrimaryKey, Column("effectId")]
		public Guid EffectId { get; set; }
	}
}