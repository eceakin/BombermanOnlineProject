namespace BombermanOnlineProject.Server.Patterns.Creational.Factory
{
	/// <summary>
	/// FACTORY PATTERN - Base Interface
	/// 
	/// Purpose: Defines the contract for all entity factories.
	/// This is the foundation of the Factory Method pattern.
	/// 
	/// Why Factory Pattern?
	/// 1. Encapsulation: Object creation logic is centralized
	/// 2. Flexibility: Easy to add new entity types
	/// 3. Open/Closed Principle: Open for extension, closed for modification
	/// 4. Decoupling: Client code doesn't need to know concrete classes
	/// 
	/// Generic Type Parameter <T>:
	/// - Allows type-safe factories for different entity types
	/// - WallFactory returns Wall types
	/// - EnemyFactory returns Enemy types
	/// - PowerUpFactory returns PowerUp types
	/// </summary>
	/// <typeparam name="T">The type of entity this factory creates</typeparam>
	public interface IEntityFactory<T>
	{
		/// <summary>
		/// Creates an entity of the specified type at the given position
		/// </summary>
		/// <param name="entityType">String identifier for the entity type (e.g., "Breakable", "Hard")</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>Created entity instance</returns>
		T Create(string entityType, int x, int y);

		/// <summary>
		/// Creates a random entity at the given position.
		/// Useful for procedural generation.
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>Randomly selected entity instance</returns>
		T CreateRandom(int x, int y);

		/// <summary>
		/// Gets all available entity types this factory can create
		/// </summary>
		/// <returns>List of entity type identifiers</returns>
		IEnumerable<string> GetAvailableTypes();
	}
}