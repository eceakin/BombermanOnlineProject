using BombermanOnline.Server.Core.Walls;
using BombermanOnlineProject.Server.Core.Walls;
using BombermanOnlineProject.Server.Patterns.Creational.Factory;

namespace BombermanOnlineProject.Server.Patterns.Creational.Factory
{
	/// <summary>
	/// FACTORY METHOD PATTERN - Concrete Implementation
	/// 
	/// Purpose: Creates different types of walls without exposing instantiation logic.
	/// 
	/// Benefits:
	/// 1. Client code uses simple string identifiers instead of constructors
	/// 2. Easy to add new wall types without changing client code
	/// 3. Centralized creation logic for walls
	/// 4. Supports random wall generation for map creation
	/// 
	/// Usage Example:
	///   var factory = new WallFactory();
	///   var wall = factory.Create("Breakable", 5, 7);
	///   var randomWall = factory.CreateRandom(3, 4);
	/// </summary>
	public class WallFactory : IEntityFactory<Wall>
	{
		#region Constants

		// Wall type identifiers
		public const string UNBREAKABLE = "Unbreakable";
		public const string BREAKABLE = "Breakable";
		public const string HARD = "Hard";

		#endregion

		#region Private Fields

		private readonly Random _random;
		private readonly Dictionary<string, Func<int, int, Wall>> _wallCreators;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes the wall factory with creation delegates
		/// </summary>
		public WallFactory()
		{
			_random = new Random();

			// Dictionary-based factory pattern for clean, extensible code
			// Adding a new wall type only requires adding an entry here
			_wallCreators = new Dictionary<string, Func<int, int, Wall>>(StringComparer.OrdinalIgnoreCase)
			{
				{ UNBREAKABLE, (x, y) => new UnbreakableWall(x, y) },
				{ BREAKABLE, (x, y) => new BreakableWall(x, y) },
				{ HARD, (x, y) => new HardWall(x, y) }
			};
		}

		#endregion

		#region IEntityFactory Implementation

		/// <summary>
		/// Creates a wall of the specified type at the given position
		/// </summary>
		/// <param name="entityType">Type of wall to create ("Unbreakable", "Breakable", "Hard")</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>Created wall instance</returns>
		/// <exception cref="ArgumentException">If entityType is invalid</exception>
		public Wall Create(string entityType, int x, int y)
		{
			if (string.IsNullOrWhiteSpace(entityType))
			{
				throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));
			}

			if (_wallCreators.TryGetValue(entityType, out var creator))
			{
				var wall = creator(x, y);
				Console.WriteLine($"[WallFactory] Created {entityType} wall at ({x}, {y})");
				return wall;
			}

			throw new ArgumentException($"Unknown wall type: {entityType}", nameof(entityType));
		}

		/// <summary>
		/// Creates a random breakable wall type (Breakable or Hard)
		/// Note: Unbreakable walls are not included in random generation
		/// as they're typically placed manually for map structure
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>Randomly selected breakable wall</returns>
		public Wall CreateRandom(int x, int y)
		{
			// Only randomize between breakable wall types
			// Unbreakable walls are placed manually for map boundaries
			var breakableTypes = new[] { BREAKABLE, HARD };
			var randomType = breakableTypes[_random.Next(breakableTypes.Length)];

			return Create(randomType, x, y);
		}

		/// <summary>
		/// Gets all available wall types this factory can create
		/// </summary>
		public IEnumerable<string> GetAvailableTypes()
		{
			return _wallCreators.Keys;
		}

		#endregion

		#region Additional Factory Methods

		/// <summary>
		/// Creates a wall with a specific theme
		/// </summary>
		/// <param name="entityType">Type of wall</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="theme">Wall theme (Desert, Forest, City)</param>
		/// <returns>Created wall with specified theme</returns>
		public Wall CreateWithTheme(string entityType, int x, int y, WallTheme theme)
		{
			var wall = Create(entityType, x, y);
			wall.Theme = theme;
			return wall;
		}

		/// <summary>
		/// Creates a breakable wall with guaranteed power-up
		/// Useful for balanced map generation
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>Breakable wall that will drop a power-up</returns>
		public BreakableWall CreateBreakableWithPowerUp(int x, int y)
		{
			return new BreakableWall(x, y, hasPowerUp: true);
		}

		/// <summary>
		/// Creates a weighted random wall based on probabilities
		/// Allows for more control over map generation
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="breakableWeight">Weight for breakable walls (default: 0.7)</param>
		/// <param name="hardWeight">Weight for hard walls (default: 0.3)</param>
		/// <returns>Wall based on weighted probability</returns>
		public Wall CreateWeightedRandom(int x, int y, float breakableWeight = 0.7f, float hardWeight = 0.3f)
		{
			// Normalize weights
			float total = breakableWeight + hardWeight;
			float normalizedBreakable = breakableWeight / total;

			// Random selection based on weights
			float roll = (float)_random.NextDouble();

			if (roll < normalizedBreakable)
			{
				return Create(BREAKABLE, x, y);
			}
			else
			{
				return Create(HARD, x, y);
			}
		}

		#endregion

		#region Validation Methods

		/// <summary>
		/// Checks if a wall type is valid
		/// </summary>
		public bool IsValidWallType(string entityType)
		{
			return _wallCreators.ContainsKey(entityType);
		}

		/// <summary>
		/// Checks if a wall type is breakable
		/// </summary>
		public bool IsBreakableType(string entityType)
		{
			return entityType.Equals(BREAKABLE, StringComparison.OrdinalIgnoreCase) ||
				   entityType.Equals(HARD, StringComparison.OrdinalIgnoreCase);
		}

		#endregion
	}
}