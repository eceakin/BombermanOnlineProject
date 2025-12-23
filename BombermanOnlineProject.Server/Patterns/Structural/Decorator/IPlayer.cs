namespace BombermanOnlineProject.Server.Patterns.Structural.Decorator
{
	/// <summary>
	/// DECORATOR PATTERN - Component Interface
	/// 
	/// Purpose: Defines the contract that both concrete Player and decorators must follow.
	/// This allows decorators to wrap players seamlessly.
	/// 
	/// Why Interface?
	/// 1. Enables polymorphism - decorators can wrap both Player and other decorators
	/// 2. Defines common operations that power-ups can enhance
	/// 3. Supports multiple decoration layers (stacking power-ups)
	/// 
	/// Design Decision:
	/// - Only includes properties/methods that can be enhanced by power-ups
	/// - Player class will implement this interface
	/// - Decorators will also implement this interface and contain an IPlayer
	/// </summary>
	public interface IPlayer
	{
		#region Core Properties

		/// <summary>
		/// Unique player identifier
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Player display name
		/// </summary>
		string PlayerName { get; }

		/// <summary>
		/// Player number (0 or 1 for two-player mode)
		/// </summary>
		int PlayerNumber { get; }

		/// <summary>
		/// Current X position on the map
		/// </summary>
		int X { get; set; }

		/// <summary>
		/// Current Y position on the map
		/// </summary>
		int Y { get; set; }

		/// <summary>
		/// Whether the player is alive
		/// </summary>
		bool IsAlive { get; set; }

		#endregion

		#region Enhanceable Properties (Decorator Targets)

		/// <summary>
		/// Movement speed - can be enhanced by SpeedBoost decorator
		/// </summary>
		float Speed { get; set; }

		/// <summary>
		/// Bomb explosion power - can be enhanced by BombPower decorator
		/// </summary>
		int BombPower { get; set; }

		/// <summary>
		/// Maximum number of bombs - can be enhanced by BombCount decorator
		/// </summary>
		int MaxBombs { get; set; }

		/// <summary>
		/// Currently active bombs
		/// </summary>
		int ActiveBombs { get; set; }

		#endregion

		#region Display Properties

		/// <summary>
		/// Character used to display the player in console
		/// </summary>
		char DisplayChar { get; }

		/// <summary>
		/// Player score
		/// </summary>
		int Score { get; set; }

		#endregion

		#region Game State Properties

		/// <summary>
		/// Whether the player is invulnerable (after respawn)
		/// </summary>
		bool IsInvulnerable { get; }

		/// <summary>
		/// Number of kills
		/// </summary>
		int Kills { get; set; }

		/// <summary>
		/// Number of deaths
		/// </summary>
		int Deaths { get; set; }

		#endregion

		#region Core Methods

		/// <summary>
		/// Updates player state each game tick
		/// </summary>
		void Update(float deltaTime);

		/// <summary>
		/// Checks if player can place a bomb
		/// </summary>
		bool CanPlaceBomb();

		/// <summary>
		/// Places a bomb at current position
		/// </summary>
		void PlaceBomb();

		/// <summary>
		/// Notifies player that a bomb has exploded
		/// </summary>
		void BombExploded();

		/// <summary>
		/// Moves player to new position
		/// </summary>
		bool MoveTo(int newX, int newY);

		/// <summary>
		/// Player takes damage
		/// </summary>
		void TakeDamage();

		/// <summary>
		/// Respawns player at spawn position
		/// </summary>
		void Respawn(int spawnX, int spawnY);

		/// <summary>
		/// Adds a kill to player stats
		/// </summary>
		void AddKill();

		#endregion
	}
}