using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
    [Table("StarterDecks")]
    public class StarterDeck : BaseModel
    {
        [PrimaryKey("id"), Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
        
        [Reference(typeof(StarterDeck_Card)), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<StarterDeck_Card>? Cards { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? CardNames { get; set; }
    }
}