using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;
using Yog.Api.Models;

[Table("Packs")]
public class Pack : BaseModel
{
	[PrimaryKey("id"), Column("id")]
	public Guid Id { get; set; }

	[Column("name")]
	public string Name { get; set; }

	[Column("cost")]
	public int Cost { get; set; }
	
	[Column("packType")]
	public int PackType { get; set; }

	[Column("createdAt"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public DateTime? CreatedAt { get; set; }
	
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? CardNames { get; set; }
}