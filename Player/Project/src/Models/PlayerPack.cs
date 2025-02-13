using Postgrest.Models;
using Postgrest.Attributes;
using System;

namespace Yog.Api.Models
{
	[Table("Player_Packs")]
	public class PlayerPack : BaseModel
	{
		[PrimaryKey("id"), Column("id")]
		public Guid Id { get; set; }

		[Column("playerId")]
		public string PlayerId { get; set; }

		[Column("packId")]
		public Guid PackId { get; set; }
	}
}