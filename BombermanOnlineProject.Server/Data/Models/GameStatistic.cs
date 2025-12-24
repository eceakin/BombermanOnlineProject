using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombermanOnlineProject.Server.Data.Models
{
	[Table("GameStatistics")]
	public class GameStatistic
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int GameStatisticId { get; set; }

		[Required]
		public int UserId { get; set; }

		[Required]
		[MaxLength(100)]
		public string SessionId { get; set; } = string.Empty;

		[Required]
		public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

		[Required]
		public bool IsWinner { get; set; }

		[Required]
		public int FinalScore { get; set; }

		[Required]
		public int Kills { get; set; }

		[Required]
		public int Deaths { get; set; }

		[Required]
		public int BombsPlaced { get; set; }

		[Required]
		public int WallsDestroyed { get; set; }

		[Required]
		public int PowerUpsCollected { get; set; }

		[Required]
		public int RoundsWon { get; set; }

		[Required]
		public int RoundsLost { get; set; }

		[Required]
		public TimeSpan GameDuration { get; set; }

		[MaxLength(50)]
		public string? OpponentUsername { get; set; }

		[MaxLength(20)]
		public string? MapTheme { get; set; }

		[ForeignKey("UserId")]
		public virtual User User { get; set; } = null!;
	}
}