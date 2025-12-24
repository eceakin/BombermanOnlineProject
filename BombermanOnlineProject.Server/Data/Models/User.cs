using BombermanOnlineProject.Server.Core.Game;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombermanOnlineProject.Server.Data.Models
{
	[Table("Users")]
	public class User
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserId { get; set; }

		[Required]
		[MaxLength(50)]
		public string Username { get; set; } = string.Empty;

		[Required]
		[MaxLength(255)]
		public string PasswordHash { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? LastLoginAt { get; set; }

		[Required]
		public bool IsActive { get; set; } = true;

		public int TotalGamesPlayed { get; set; } = 0;

		public int TotalWins { get; set; } = 0;

		public int TotalLosses { get; set; } = 0;

		public int TotalKills { get; set; } = 0;

		public int TotalDeaths { get; set; } = 0;

		public int HighestScore { get; set; } = 0;

		[MaxLength(20)]
		public string? PreferredTheme { get; set; }
		public virtual ICollection<GameStatistic> GameStatistics { get; set; } = new List<GameStatistic>();

		public virtual ICollection<HighScore> HighScores { get; set; } = new List<HighScore>();

		public virtual PlayerPreference? PlayerPreference { get; set; }
	}
}