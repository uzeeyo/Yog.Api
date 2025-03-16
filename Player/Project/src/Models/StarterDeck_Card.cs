using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
    [Table("StarterDeck_Cards")]
    public class StarterDeck_Card : BaseModel
    {
        [PrimaryKey("id"), Column("id")]
        public Guid Id { get; set; }

        [Column("deckId")]
        public Guid DeckId { get; set; }

        [Column("cardName")]
        public string CardName { get; set; }
    }
}