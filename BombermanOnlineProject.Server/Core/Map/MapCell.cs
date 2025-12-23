using BombermanOnlineProject.Server.Core.Entities;
using BombermanOnlineProject.Server.Core.Walls;
namespace BombermanOnlineProject.Server.Core.Map
{
	/// <summary>
	/// Represents a single cell in the game map grid.
	/// 
	/// Design Considerations:
	/// - Each cell can contain multiple objects (wall, player, bomb, power-up)
	/// - Thread-safe operations for multiplayer environment
	/// - Efficient collision detection and pathfinding
	/// 
	/// Cell States:
	/// - Empty: Walkable, no objects
	/// - Wall: Contains a wall (may be walkable with power-up)
	/// - Occupied: Contains player, bomb, or enemy
	/// - PowerUp: Contains a collectible power-up
	/// </summary>
	public class MapCell
	{
		#region Properties

		/// <summary>
		/// X coordinate of this cell in the map
		/// </summary>
		public int X { get; private set; }

		/// <summary>
		/// Y coordinate of this cell in the map
		/// </summary>
		public int Y { get; private set; }

		/// <summary>
		/// Wall object in this cell (null if no wall)
		/// </summary>
		public Wall? Wall { get; private set; }

		/// <summary>
		/// Power-up in this cell (null if no power-up)
		/// </summary>
		public GameObject? PowerUp { get; private set; }

		/// <summary>
		/// List of game objects currently in this cell (players, bombs, enemies)
		/// Using a list allows multiple objects at the same position temporarily
		/// </summary>
		private readonly List<GameObject> _objects = new();

		/// <summary>
		/// Thread-safety lock for concurrent access
		/// </summary>
		private readonly object _lock = new();

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new map cell at the specified position
		/// </summary>
		public MapCell(int x, int y)
		{
			X = x;
			Y = y;
		}

		#endregion

		#region Wall Management

		/// <summary>
		/// Places a wall in this cell
		/// </summary>
		public void SetWall(Wall wall)
		{
			lock (_lock)
			{
				Wall = wall;
			}
		}

		/// <summary>
		/// Removes the wall from this cell (when destroyed)
		/// </summary>
		public void RemoveWall()
		{
			lock (_lock)
			{
				Wall = null;
			}
		}

		/// <summary>
		/// Checks if this cell has a wall
		/// </summary>
		public bool HasWall()
		{
			lock (_lock)
			{
				return Wall != null && Wall.IsAlive;
			}
		}

		/// <summary>
		/// Checks if this cell has an unbreakable wall
		/// </summary>
		public bool HasUnbreakableWall()
		{
			lock (_lock)
			{
				return Wall != null && Wall.IsAlive && !Wall.IsBreakable;
			}
		}

		#endregion

		#region PowerUp Management

		/// <summary>
		/// Places a power-up in this cell
		/// </summary>
		public void SetPowerUp(GameObject powerUp)
		{
			lock (_lock)
			{
				PowerUp = powerUp;
			}
		}

		/// <summary>
		/// Removes and returns the power-up from this cell
		/// </summary>
		public GameObject? CollectPowerUp()
		{
			lock (_lock)
			{
				var powerUp = PowerUp;
				PowerUp = null;
				return powerUp;
			}
		}

		/// <summary>
		/// Checks if this cell has a power-up
		/// </summary>
		public bool HasPowerUp()
		{
			lock (_lock)
			{
				return PowerUp != null && PowerUp.IsAlive;
			}
		}

		#endregion

		#region Object Management

		/// <summary>
		/// Adds a game object to this cell
		/// </summary>
		public void AddObject(GameObject obj)
		{
			lock (_lock)
			{
				if (!_objects.Contains(obj))
				{
					_objects.Add(obj);
				}
			}
		}

		/// <summary>
		/// Removes a game object from this cell
		/// </summary>
		public void RemoveObject(GameObject obj)
		{
			lock (_lock)
			{
				_objects.Remove(obj);
			}
		}

		/// <summary>
		/// Gets all game objects in this cell
		/// </summary>
		public List<GameObject> GetObjects()
		{
			lock (_lock)
			{
				return new List<GameObject>(_objects);
			}
		}

		/// <summary>
		/// Gets objects of a specific type
		/// </summary>
		public List<GameObject> GetObjectsOfType(GameObjectType type)
		{
			lock (_lock)
			{
				return _objects.Where(obj => obj.Type == type && obj.IsAlive).ToList();
			}
		}

		/// <summary>
		/// Checks if this cell contains any object of the specified type
		/// </summary>
		public bool HasObjectOfType(GameObjectType type)
		{
			lock (_lock)
			{
				return _objects.Any(obj => obj.Type == type && obj.IsAlive);
			}
		}

		#endregion

		#region Pathfinding & Collision

		/// <summary>
		/// Checks if this cell is walkable (no walls, no bombs)
		/// </summary>
		public bool IsWalkable()
		{
			lock (_lock)
			{
				// Cell is walkable if:
				// 1. No wall OR wall is destroyed
				// 2. No bomb (bombs block movement)
				bool hasWall = Wall != null && Wall.IsAlive;
				bool hasBomb = _objects.Any(obj => obj.Type == GameObjectType.Bomb && obj.IsAlive);

				return !hasWall && !hasBomb;
			}
		}

		/// <summary>
		/// Checks if explosion can pass through this cell
		/// </summary>
		public bool IsExplosionPassable()
		{
			lock (_lock)
			{
				// Explosion stops at unbreakable walls
				return Wall == null || Wall.IsBreakable;
			}
		}

		/// <summary>
		/// Checks if this cell is completely empty
		/// </summary>
		public bool IsEmpty()
		{
			lock (_lock)
			{
				return Wall == null && PowerUp == null && _objects.Count == 0;
			}
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Clears all objects from this cell (except walls)
		/// </summary>
		public void ClearObjects()
		{
			lock (_lock)
			{
				_objects.Clear();
				PowerUp = null;
			}
		}

		/// <summary>
		/// Gets a summary of the cell's contents for debugging
		/// </summary>
		public string GetCellInfo()
		{
			lock (_lock)
			{
				var info = $"Cell ({X}, {Y}): ";

				if (Wall != null)
					info += $"Wall[{Wall.GetType().Name}] ";

				if (PowerUp != null)
					info += "PowerUp ";

				if (_objects.Count > 0)
					info += $"Objects[{_objects.Count}] ";

				return info.Trim();
			}
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"MapCell({X}, {Y}) - Wall: {HasWall()}, Objects: {_objects.Count}";
		}

		#endregion
	}
}