using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BombermanOnlineProject.Server.Data.Models
{
	[Table("PlayerPreferences")]
	public class PlayerPreference
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int PlayerPreferenceId { get; set; }

		[Required]
		public int UserId { get; set; }

		[MaxLength(20)]
		public string PreferredTheme { get; set; } = "Forest";

		[MaxLength(20)]
		public string PreferredDifficulty { get; set; } = "Normal";

		public bool SoundEnabled { get; set; } = true;

		public bool MusicEnabled { get; set; } = true;

		public int SoundVolume { get; set; } = 70;

		public int MusicVolume { get; set; } = 50;

		public bool ShowGrid { get; set; } = true;

		public bool ShowFPS { get; set; } = false;

		public bool EnableParticles { get; set; } = true;

		public bool AutoSaveReplays { get; set; } = false;

		[MaxLength(10)]
		public string KeyMoveUp { get; set; } = "W";

		[MaxLength(10)]
		public string KeyMoveDown { get; set; } = "S";

		[MaxLength(10)]
		public string KeyMoveLeft { get; set; } = "A";

		[MaxLength(10)]
		public string KeyMoveRight { get; set; } = "D";

		[MaxLength(10)]
		public string KeyPlaceBomb { get; set; } = "Space";

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey("UserId")]
		public virtual User User { get; set; } = null!;
	}
}