using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Walls;

namespace BombermanOnlineProject.Server.Core.Walls
{
	/// <summary>
	/// Breakable wall that is destroyed by a single explosion.
	/// May drop power-ups when destroyed.
	/// 
	/// Characteristics:
	/// - Destroyed with one explosion
	/// - Can drop power-ups (based on probability)
	/// - Provides strategic gameplay elements
	/// - Creates dynamic map changes
	/// </summary>
	public class BreakableWall : Wall
	{
		#region Constants

		private const char DEFAULT_DISPLAY_CHAR = '▒';
		private const int DEFAULT_HP = 1;

		#endregion

		#region Properties

		/// <summary>
		/// Indicates if this wall will drop a power-up when destroyed
		/// Determined at creation time based on GameSettings.PowerUpDropChance
		/// </summary>
		public bool HasPowerUp { get; private set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a breakable wall at the specified position
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="hasPowerUp">Whether this wall contains a power-up (optional, auto-determined if not specified)</param>
		public BreakableWall(int x, int y, bool? hasPowerUp = null)
			: base(x, y, isBreakable: true, hitPoints: DEFAULT_HP, displayChar: DEFAULT_DISPLAY_CHAR)
		{
			// Determine power-up drop based on probability or explicit parameter
			if (hasPowerUp.HasValue)
			{
				HasPowerUp = hasPowerUp.Value;
			}
			else
			{
				// Random chance based on game settings
				var random = new Random();
				HasPowerUp = random.NextDouble() < GameSettings.PowerUpDropChance;
			}
		}

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Takes damage from an explosion.
		/// Breakable walls are destroyed in one hit.
		/// </summary>
		/// <returns>True if the wall is destroyed</returns>
		public override bool TakeDamage()
		{
			HitPoints--;

			if (HitPoints <= 0)
			{
				Destroy();
				return true; // Wall is destroyed
			}

			return false;
		}

		/// <summary>
		/// Called when the wall is destroyed.
		/// Handles power-up spawn logic.
		/// </summary>
		public override void Destroy()
		{
			base.Destroy();

			if (HasPowerUp)
			{
				Console.WriteLine($"[BreakableWall] Power-up will spawn at ({X}, {Y})");
				// Power-up spawning will be handled by GameSession/GameMap
			}
		}

		#endregion

		#region Display Methods

		/// <summary>
		/// Gets the display character based on the theme
		/// </summary>
		public char GetThemedDisplayChar()
		{
			return Theme switch
			{
				WallTheme.Desert => '░',   // Light sand pattern
				WallTheme.Forest => '♠',   // Bush/vegetation
				WallTheme.City => '▓',     // Brick pattern
				_ => DEFAULT_DISPLAY_CHAR
			};
		}

		#endregion
	}
}