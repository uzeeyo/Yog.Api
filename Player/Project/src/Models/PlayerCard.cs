using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
	[Table("Player_Cards")]
	public class PlayerCard : BaseModel
	{
		[Column("playerId")]
		public string PlayerId { get; set; }
		
		[Column("cardName")]
		public string CardName { get; set; }
		
		[Column("count")]
		public int Count { get; set; }
	}
}