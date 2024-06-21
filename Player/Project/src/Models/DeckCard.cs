using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
	[Table("Deck_Cards")]
	public class DeckCard : BaseModel
	{
		[PrimaryKey("id")]
		public string Id { get; set; }
		
		[Column("deckId")]
		public Guid DeckId { get; set; }
		
		[Column("cardId")]
		public Guid CardId { get; set; }
	}
}