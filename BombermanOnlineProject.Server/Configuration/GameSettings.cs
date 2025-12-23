namespace BombermanOnlineProject.Server.Configuration
{
	/// <summary>
	/// Global game configuration and constants.
	/// This class holds all game-related settings that will be used throughout the application.
	/// Using static readonly ensures these values are initialized once and cannot be changed at runtime.
	/// </summary>
	public static class GameSettings
	{
		#region Map Configuration

		/// <summary>
		/// Width of the game map (number of cells horizontally)
		/// </summary>
		public static readonly int MapWidth = 15;

		/// <summary>
		/// Height of the game map (number of cells vertically)
		/// </summary>
		public static readonly int MapHeight = 13;

		#endregion

		#region Bomb Configuration

		/// <summary>
		/// Time in milliseconds before a bomb explodes after being placed
		/// </summary>
		public static readonly int BombTimer = 3000; // 3 seconds

		/// <summary>
		/// Default explosion range (number of cells in each direction)
		/// </summary>
		public static readonly int DefaultBombPower = 2;

		/// <summary>
		/// Default number of bombs a player can place simultaneously
		/// </summary>
		public static readonly int DefaultBombCount = 1;

		/// <summary>
		/// Maximum bomb power a player can achieve
		/// </summary>
		public static readonly int MaxBombPower = 8;

		/// <summary>
		/// Maximum simultaneous bombs a player can have
		/// </summary>
		public static readonly int MaxBombCount = 8;

		/// <summary>
		/// Duration of explosion animation in milliseconds
		/// </summary>
		public static readonly int ExplosionDuration = 500;

		#endregion

		#region Player Configuration

		/// <summary>
		/// Default movement speed in cells per second
		/// </summary>
		public static readonly float DefaultPlayerSpeed = 3.0f;

		/// <summary>
		/// Maximum player speed achievable with power-ups
		/// </summary>
		public static readonly float MaxPlayerSpeed = 6.0f;

		/// <summary>
		/// Player invulnerability duration after respawn (milliseconds)
		/// </summary>
		public static readonly int InvulnerabilityDuration = 2000;

		#endregion

		#region PowerUp Configuration

		/// <summary>
		/// Probability (0.0 to 1.0) that a power-up drops when a breakable wall is destroyed
		/// </summary>
		public static readonly float PowerUpDropChance = 0.4f; // 40%

		/// <summary>
		/// Speed boost multiplier
		/// </summary>
		public static readonly float SpeedBoostMultiplier = 1.5f;

		/// <summary>
		/// Bomb power increase amount per power-up
		/// </summary>
		public static readonly int BombPowerIncrement = 1;

		/// <summary>
		/// Bomb count increase amount per power-up
		/// </summary>
		public static readonly int BombCountIncrement = 1;

		#endregion

		#region Wall Configuration

		/// <summary>
		/// Hit points required to destroy a hard wall
		/// </summary>
		public static readonly int HardWallHP = 3;

		/// <summary>
		/// Percentage of breakable walls on the map (0.0 to 1.0)
		/// </summary>
		public static readonly float BreakableWallDensity = 0.3f; // 30%

		#endregion

		#region Enemy Configuration

		/// <summary>
		/// Default enemy movement speed in cells per second
		/// </summary>
		public static readonly float DefaultEnemySpeed = 2.0f;

		/// <summary>
		/// Chasing enemy speed in cells per second
		/// </summary>
		public static readonly float ChasingEnemySpeed = 2.5f;

		/// <summary>
		/// Enemy AI update interval in milliseconds
		/// </summary>
		public static readonly int EnemyAIUpdateInterval = 500;

		/// <summary>
		/// Detection range for chasing enemies (in cells)
		/// </summary>
		public static readonly int ChasingEnemyDetectionRange = 5;

		#endregion

		#region Game Rules

		/// <summary>
		/// Number of rounds to win the match
		/// </summary>
		public static readonly int RoundsToWin = 3;

		/// <summary>
		/// Maximum game duration in seconds (0 = unlimited)
		/// </summary>
		public static readonly int MaxGameDuration = 300; // 5 minutes

		/// <summary>
		/// Delay before starting a new round (milliseconds)
		/// </summary>
		public static readonly int RoundStartDelay = 3000;

		#endregion

		#region Network Configuration

		/// <summary>
		/// Maximum number of players per game session
		/// </summary>
		public static readonly int MaxPlayersPerGame = 2;

		/// <summary>
		/// Server tick rate (updates per second)
		/// </summary>
		public static readonly int ServerTickRate = 30;

		/// <summary>
		/// Client-server synchronization interval (milliseconds)
		/// </summary>
		public static readonly int SyncInterval = 100;

		#endregion

		#region Theme Configuration

		/// <summary>
		/// Available game themes
		/// </summary>
		public enum GameTheme
		{
			Desert,
			Forest,
			City
		}

		/// <summary>
		/// Default game theme
		/// </summary>
		public static readonly GameTheme DefaultTheme = GameTheme.Forest;

		#endregion
	}
}