using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace Yog.Api.Models
{
	[Table("Operators")]
	public class Operator : BaseModel
	{
		[PrimaryKey("id"), Column("id")]
		public Guid Id { get; set; }

		[Column("name")]
		public string Name { get; set; }

		[Column("description")]
		public string Description { get; set; }

		[Column("health")]
		public int Health { get; set; }

		[Column("memory")]
		public int Memory { get; set; }

		[Column("shields")]
		public int? Shields { get; set; }

		[Column("shieldRegen")]
		public int? ShieldRegen { get; set; }
	}
}