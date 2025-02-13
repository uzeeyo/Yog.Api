using Postgrest.Attributes;
using Postgrest.Models;
using System;

namespace Yog.Api.Models
{
	[Table("MatchQueue")]
	public class MatchQueue : BaseModel
	{
		[PrimaryKey("id")]
		public Guid Id { get; set; }

		[Column("playerId")]
		public string PlayerId { get; set; }

		[Column("createdAt")]
		public DateTime CreatedAt { get; set; }

		[Column("connecting")]
		public bool Connecting { get; set; }

		[Column("allocationId")]
		public string AllocationId { get; set; }
	}
}