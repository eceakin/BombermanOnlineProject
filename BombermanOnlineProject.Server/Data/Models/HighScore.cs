using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombermanOnlineProject.Server.Data.Models
{
	[Table("HighScores")]
	public class HighScore
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int HighScoreId { get; set; }

		[Required]
		public int UserId { get; set; }

		[Required]
		public int Score { get; set; }

		[Required]
		public DateTime AchievedAt { get; set; } = DateTime.UtcNow;

		[Required]
		[MaxLength(100)]
		public string SessionId { get; set; } = string.Empty;

		[Required]
		public int Kills { get; set; }

		[Required]
		public int Deaths { get; set; }

		[Required]
		public TimeSpan GameDuration { get; set; }

		[MaxLength(50)]
		public string? OpponentUsername { get; set; }

		[MaxLength(20)]
		public string? MapTheme { get; set; }

		public int RoundsWon { get; set; }

		[ForeignKey("UserId")]
		public virtual User User { get; set; } = null!;
	}
}