namespace BombermanOnlineProject.Server.Core.Entities
{
	/// <summary>
	/// Base abstract class for all game entities.
	/// 
	/// Design Principle: 
	/// - DRY (Don't Repeat Yourself): Common properties/methods in one place
	/// - Inheritance: All game objects share position, ID, and lifecycle
	/// - Polymorphism: Different entities can override Update() behavior
	/// 
	/// This serves as the foundation for:
	/// - Player
	/// - Bomb
	/// - Enemy
	/// - PowerUp
	/// - Explosion (temporary object)
	/// </summary>
	public abstract class GameObject
	{
		#region Properties

		/// <summary>
		/// Unique identifier for this game object
		/// </summary>
		public string Id { get; protected set; }

		/// <summary>
		/// X coordinate on the game map (column)
		/// </summary>
		public int X { get; set; }

		/// <summary>
		/// Y coordinate on the game map (row)
		/// </summary>
		public int Y { get; set; }

		/// <summary>
		/// Indicates whether this object is currently active/alive
		/// When false, the object should be removed from the game
		/// </summary>
		public bool IsAlive { get; set; }

		/// <summary>
		/// Type of the game object (for identification and rendering)
		/// </summary>
		public GameObjectType Type { get; protected set; }

		/// <summary>
		/// Timestamp when this object was created
		/// </summary>
		public DateTime CreatedAt { get; protected set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new game object with position
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="type">Type of the game object</param>
		protected GameObject(int x, int y, GameObjectType type)
		{
			Id = Guid.NewGuid().ToString();
			X = x;
			Y = y;
			Type = type;
			IsAlive = true;
			CreatedAt = DateTime.UtcNow;
		}

		#endregion

		#region Abstract Methods

		/// <summary>
		/// Update method called every game tick.
		/// Each derived class implements its own update logic.
		/// </summary>
		/// <param name="deltaTime">Time elapsed since last update (in seconds)</param>
		public abstract void Update(float deltaTime);

		#endregion

		#region Virtual Methods

		/// <summary>
		/// Called when this object is destroyed/removed from the game.
		/// Can be overridden for cleanup logic.
		/// </summary>
		public virtual void Destroy()
		{
			IsAlive = false;
			Console.WriteLine($"[GameObject] {Type} at ({X}, {Y}) destroyed");
		}

		/// <summary>
		/// Checks if this object collides with another object at the same position
		/// </summary>
		public virtual bool CollidesWith(GameObject other)
		{
			return X == other.X && Y == other.Y && IsAlive && other.IsAlive;
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Calculates Manhattan distance to another game object
		/// Useful for AI pathfinding and detection
		/// </summary>
		public int DistanceTo(GameObject other)
		{
			return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
		}

		/// <summary>
		/// Calculates Manhattan distance to a specific position
		/// </summary>
		public int DistanceTo(int targetX, int targetY)
		{
			return Math.Abs(X - targetX) + Math.Abs(Y - targetY);
		}

		/// <summary>
		/// Checks if this object is at a specific position
		/// </summary>
		public bool IsAt(int x, int y)
		{
			return X == x && Y == y;
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{Type} [ID: {Id.Substring(0, 8)}...] at ({X}, {Y}) - Alive: {IsAlive}";
		}

		#endregion
	}

	/// <summary>
	/// Enumeration of all possible game object types.
	/// Used for identification, rendering, and collision detection.
	/// </summary>
	public enum GameObjectType
	{
		Player,
		Bomb,
		Explosion,
		Enemy,
		PowerUp,
		Wall
	}
}