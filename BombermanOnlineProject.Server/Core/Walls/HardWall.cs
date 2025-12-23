using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Walls;

namespace BombermanOnline.Server.Core.Walls
{
	/// <summary>
	/// Hard wall that requires multiple explosions to destroy.
	/// Provides additional strategic depth and challenges.
	/// 
	/// Characteristics:
	/// - Requires 3 hits to destroy (configurable via GameSettings)
	/// - Visual feedback shows damage state
	/// - May drop power-ups when destroyed
	/// - Creates chokepoints and strategic positions
	/// </summary>
	public class HardWall : Wall
	{
		#region Constants

		private const char DEFAULT_DISPLAY_CHAR = '▓';

		#endregion

		#region Properties

		/// <summary>
		/// Indicates if this wall will drop a power-up when destroyed
		/// </summary>
		public bool HasPowerUp { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a hard wall at the specified position
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="hasPowerUp">Whether this wall contains a power-up (optional)</param>
		public HardWall(int x, int y, bool? hasPowerUp = null)
			: base(x, y,
				   isBreakable: true,
				   hitPoints: GameSettings.HardWallHP,
				   displayChar: DEFAULT_DISPLAY_CHAR)
		{
			// Determine power-up drop (lower chance than breakable walls)
			if (hasPowerUp.HasValue)
			{
				HasPowerUp = hasPowerUp.Value;
			}
			else
			{
				// 20% chance for hard walls (half of normal breakable walls)
				var random = new Random();
				HasPowerUp = random.NextDouble() < (GameSettings.PowerUpDropChance * 0.5f);
			}
		}

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Takes damage from an explosion.
		/// Hard walls require multiple hits to destroy.
		/// </summary>
		/// <returns>True if the wall is destroyed</returns>
		public override bool TakeDamage()
		{
			HitPoints--;

			Console.WriteLine($"[HardWall] Wall at ({X}, {Y}) took damage. HP: {HitPoints}/{MaxHitPoints}");

			if (HitPoints <= 0)
			{
				Destroy();
				return true; // Wall is destroyed
			}

			// Wall is damaged but not destroyed
			return false;
		}

		/// <summary>
		/// Called when the wall is destroyed after taking enough damage
		/// </summary>
		public override void Destroy()
		{
			base.Destroy();

			if (HasPowerUp)
			{
				Console.WriteLine($"[HardWall] Power-up will spawn at ({X}, {Y})");
				// Power-up spawning will be handled by GameSession/GameMap
			}
		}

		#endregion

		#region Display Methods

		/// <summary>
		/// Gets the display character based on damage state and theme.
		/// Visual feedback shows how damaged the wall is.
		/// </summary>
		public char GetDisplayCharByDamage()
		{
			float healthPercentage = GetHealthPercentage();

			// Progressive damage visualization
			if (healthPercentage > 0.66f)
			{
				// Healthy state (3/3 HP)
				return Theme switch
				{
					WallTheme.Desert => '▓',
					WallTheme.Forest => '♦',
					WallTheme.City => '■',
					_ => '▓'
				};
			}
			else if (healthPercentage > 0.33f)
			{
				// Damaged state (2/3 HP)
				return Theme switch
				{
					WallTheme.Desert => '▒',
					WallTheme.Forest => '♣',
					WallTheme.City => '▒',
					_ => '▒'
				};
			}
			else
			{
				// Critical state (1/3 HP)
				return Theme switch
				{
					WallTheme.Desert => '░',
					WallTheme.Forest => '♠',
					WallTheme.City => '░',
					_ => '░'
				};
			}
		}

		/// <summary>
		/// Gets a color indicator for the damage state (for future rendering)
		/// </summary>
		public string GetDamageColor()
		{
			float healthPercentage = GetHealthPercentage();

			if (healthPercentage > 0.66f) return "Green";    // Healthy
			if (healthPercentage > 0.33f) return "Yellow";   // Damaged
			return "Red";                                     // Critical
		}

		#endregion
	}
}