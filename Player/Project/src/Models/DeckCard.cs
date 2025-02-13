using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
	[Table("Deck_Cards")]
	public class 	DeckCard : BaseModel
	{
		[PrimaryKey("id")]
		public Guid Id { get; set; }
		
		[Column("deckId")]
		public Guid DeckId { get; set; }
		
		[Column("cardName")]
		public string CardName { get; set; }
	}
}