using BombermanOnlineProject.Server.Core.Entities;
using BombermanOnlineProject.Server.Core.Entities;

namespace BombermanOnlineProject.Server.Core.Walls
{
	/// <summary>
	/// Abstract base class for all wall types in the game.
	/// 
	/// Design Pattern: Template Method Pattern (implicit)
	/// - Defines the structure for walls
	/// - Derived classes implement specific behavior
	/// 
	/// Wall Types:
	/// 1. UnbreakableWall - Cannot be destroyed (map boundaries, obstacles)
	/// 2. BreakableWall - Destroyed with single explosion (may drop power-ups)
	/// 3. HardWall - Requires multiple explosions to destroy
	/// </summary>
	public abstract class Wall : GameObject
	{
		#region Properties

		/// <summary>
		/// Indicates if this wall can be destroyed by explosions
		/// </summary>
		public bool IsBreakable { get; protected set; }

		/// <summary>
		/// Current hit points of the wall
		/// Only relevant for breakable walls
		/// </summary>
		public int HitPoints { get; protected set; }

		/// <summary>
		/// Maximum hit points this wall type can have
		/// </summary>
		public int MaxHitPoints { get; protected set; }

		/// <summary>
		/// Visual representation character for console rendering
		/// </summary>
		public char DisplayChar { get; protected set; }

		/// <summary>
		/// Wall theme variant (Desert, Forest, City)
		/// </summary>
		public WallTheme Theme { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a wall at the specified position
		/// </summary>
		protected Wall(int x, int y, bool isBreakable, int hitPoints, char displayChar)
			: base(x, y, GameObjectType.Wall)
		{
			IsBreakable = isBreakable;
			HitPoints = hitPoints;
			MaxHitPoints = hitPoints;
			DisplayChar = displayChar;
			Theme = WallTheme.Default;
		}

		#endregion

		#region Abstract Methods

		/// <summary>
		/// Called when this wall is hit by an explosion.
		/// Returns true if the wall is destroyed.
		/// </summary>
		public abstract bool TakeDamage();

		#endregion

		#region Virtual Methods

		/// <summary>
		/// Updates the wall state (most walls are static, but can be overridden)
		/// </summary>
		public override void Update(float deltaTime)
		{
			// Most walls don't need updates, but method is here for extensibility
			// Example: Animated walls, degrading walls over time, etc.
		}

		/// <summary>
		/// Called when the wall is destroyed
		/// Can be overridden for special effects or power-up drops
		/// </summary>
		public override void Destroy()
		{
			base.Destroy();
			Console.WriteLine($"[Wall] {GetType().Name} at ({X}, {Y}) destroyed");
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Gets the health percentage (for visual feedback)
		/// </summary>
		public float GetHealthPercentage()
		{
			if (MaxHitPoints == 0) return 0f;
			return (float)HitPoints / MaxHitPoints;
		}

		/// <summary>
		/// Checks if the wall is at critical health
		/// </summary>
		public bool IsCriticalHealth()
		{
			return IsBreakable && HitPoints <= 1;
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			var breakableStr = IsBreakable ? $"HP: {HitPoints}/{MaxHitPoints}" : "Unbreakable";
			return $"{GetType().Name} at ({X}, {Y}) - {breakableStr}";
		}

		#endregion
	}

	/// <summary>
	/// Wall theme variants for different map aesthetics
	/// </summary>
	public enum WallTheme
	{
		Default,
		Desert,    // Sand, stone
		Forest,    // Trees, wood
		City       // Concrete, brick
	}
}