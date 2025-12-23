
using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Map;
using BombermanOnlineProject.Server.Core.Walls;
using BombermanOnlineProject.Server.Patterns.Creational.Factory;

namespace BombermanOnline.Server.Core.Map
{
	/// <summary>
	/// Manages the game map grid, wall placement, and spatial queries.
	/// 
	/// Responsibilities:
	/// 1. Map generation with walls
	/// 2. Spatial indexing for fast lookups
	/// 3. Pathfinding support
	/// 4. Collision detection
	/// 
	/// Map Layout (Standard Bomberman):
	/// - Borders: Unbreakable walls
	/// - Even coordinates: Unbreakable walls (create maze structure)
	/// - Odd coordinates: Walkable spaces with breakable walls
	/// - Player spawn areas: Protected zones without walls
	/// </summary>
	public class GameMap
	{
		#region Properties

		/// <summary>
		/// Width of the map in cells
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Height of the map in cells
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		/// Current theme of the map
		/// </summary>
		public GameSettings.GameTheme Theme { get; private set; }

		/// <summary>
		/// 2D grid of map cells
		/// </summary>
		private readonly MapCell[,] _grid;

		/// <summary>
		/// Factory for creating walls
		/// </summary>
		private readonly WallFactory _wallFactory;

		/// <summary>
		/// Random number generator for map generation
		/// </summary>
		private readonly Random _random;

		#endregion

		#region Player Spawn Positions

		/// <summary>
		/// Standard spawn positions for 2-player mode
		/// Top-left and bottom-right corners
		/// </summary>
		public static readonly (int X, int Y)[] SpawnPositions = new[]
		{
			(1, 1),                                                    // Player 1: Top-left
            (GameSettings.MapWidth - 2, GameSettings.MapHeight - 2)   // Player 2: Bottom-right
        };

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new game map
		/// </summary>
		/// <param name="width">Map width (default from settings)</param>
		/// <param name="height">Map height (default from settings)</param>
		/// <param name="theme">Map theme (default from settings)</param>
		public GameMap(
			int? width = null,
			int? height = null,
			GameSettings.GameTheme? theme = null)
		{
			Width = width ?? GameSettings.MapWidth;
			Height = height ?? GameSettings.MapHeight;
			Theme = theme ?? GameSettings.DefaultTheme;

			_grid = new MapCell[Width, Height];
			_wallFactory = new WallFactory();
			_random = new Random();

			InitializeGrid();
			GenerateMap();

			Console.WriteLine($"[GameMap] Map created: {Width}x{Height}, Theme: {Theme}");
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Initializes all map cells
		/// </summary>
		private void InitializeGrid()
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					_grid[x, y] = new MapCell(x, y);
				}
			}
		}

		/// <summary>
		/// Generates the map with walls following classic Bomberman layout
		/// </summary>
		private void GenerateMap()
		{
			// Step 1: Place border walls (unbreakable)
			PlaceBorderWalls();

			// Step 2: Place structural walls at even coordinates (classic maze pattern)
			PlaceStructuralWalls();

			// Step 3: Place random breakable walls
			PlaceBreakableWalls();

			// Step 4: Clear player spawn areas
			ClearSpawnAreas();
		}

		/// <summary>
		/// Places unbreakable walls around the map border
		/// </summary>
		private void PlaceBorderWalls()
		{
			for (int x = 0; x < Width; x++)
			{
				PlaceWall(WallFactory.UNBREAKABLE, x, 0);           // Top border
				PlaceWall(WallFactory.UNBREAKABLE, x, Height - 1);  // Bottom border
			}

			for (int y = 0; y < Height; y++)
			{
				PlaceWall(WallFactory.UNBREAKABLE, 0, y);           // Left border
				PlaceWall(WallFactory.UNBREAKABLE, Width - 1, y);   // Right border
			}
		}

		/// <summary>
		/// Places structural unbreakable walls at even coordinates
		/// Creates the classic Bomberman maze pattern
		/// </summary>
		private void PlaceStructuralWalls()
		{
			for (int x = 2; x < Width - 1; x += 2)
			{
				for (int y = 2; y < Height - 1; y += 2)
				{
					PlaceWall(WallFactory.UNBREAKABLE, x, y);
				}
			}
		}

		/// <summary>
		/// Places random breakable walls in walkable spaces
		/// Follows the density setting from GameSettings
		/// </summary>
		private void PlaceBreakableWalls()
		{
			for (int x = 1; x < Width - 1; x++)
			{
				for (int y = 1; y < Height - 1; y++)
				{
					// Skip cells that already have walls
					if (GetCell(x, y).HasWall())
						continue;

					// Skip spawn areas (will be cleared later, but optimize here)
					if (IsInSpawnArea(x, y))
						continue;

					// Place breakable wall based on density probability
					if (_random.NextDouble() < GameSettings.BreakableWallDensity)
					{
						// Use weighted random: more breakable walls than hard walls
						var wall = _wallFactory.CreateWeightedRandom(x, y,
							breakableWeight: 0.75f,
							hardWeight: 0.25f);

						wall.Theme = ConvertTheme(Theme);
						GetCell(x, y).SetWall(wall);
					}
				}
			}
		}

		/// <summary>
		/// Clears walls around player spawn positions
		/// Ensures players have safe starting areas
		/// </summary>
		private void ClearSpawnAreas()
		{
			foreach (var (spawnX, spawnY) in SpawnPositions)
			{
				// Clear 3x3 area around spawn point
				for (int dx = -1; dx <= 1; dx++)
				{
					for (int dy = -1; dy <= 1; dy++)
					{
						int x = spawnX + dx;
						int y = spawnY + dy;

						if (IsValidPosition(x, y))
						{
							var cell = GetCell(x, y);

							// Only remove breakable walls, keep structural walls
							if (cell.Wall != null && cell.Wall.IsBreakable)
							{
								cell.RemoveWall();
							}
						}
					}
				}
			}
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Places a wall at the specified position
		/// </summary>
		private void PlaceWall(string wallType, int x, int y)
		{
			var wall = _wallFactory.CreateWithTheme(wallType, x, y, ConvertTheme(Theme));
			GetCell(x, y).SetWall(wall);
		}

		/// <summary>
		/// Checks if a position is within a spawn area
		/// </summary>
		private bool IsInSpawnArea(int x, int y)
		{
			foreach (var (spawnX, spawnY) in SpawnPositions)
			{
				// Check if within 2-cell radius of spawn
				if (Math.Abs(x - spawnX) <= 2 && Math.Abs(y - spawnY) <= 2)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Converts GameSettings.GameTheme to WallTheme
		/// </summary>
		private WallTheme ConvertTheme(GameSettings.GameTheme theme)
		{
			return theme switch
			{
				GameSettings.GameTheme.Desert => WallTheme.Desert,
				GameSettings.GameTheme.Forest => WallTheme.Forest,
				GameSettings.GameTheme.City => WallTheme.City,
				_ => WallTheme.Default
			};
		}

		#endregion

		#region Public Access Methods

		/// <summary>
		/// Gets the cell at the specified position
		/// </summary>
		public MapCell GetCell(int x, int y)
		{
			if (!IsValidPosition(x, y))
			{
				throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of map bounds");
			}

			return _grid[x, y];
		}

		/// <summary>
		/// Checks if a position is within map bounds
		/// </summary>
		public bool IsValidPosition(int x, int y)
		{
			return x >= 0 && x < Width && y >= 0 && y < Height;
		}

		/// <summary>
		/// Checks if a position is walkable (no walls, no bombs)
		/// </summary>
		public bool IsWalkable(int x, int y)
		{
			if (!IsValidPosition(x, y))
				return false;

			return GetCell(x, y).IsWalkable();
		}

		/// <summary>
		/// Gets all walkable neighbors of a position (for pathfinding)
		/// </summary>
		public List<(int X, int Y)> GetWalkableNeighbors(int x, int y)
		{
			var neighbors = new List<(int, int)>();

			// Check 4 directions: up, down, left, right
			var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

			foreach (var (dx, dy) in directions)
			{
				int newX = x + dx;
				int newY = y + dy;

				if (IsWalkable(newX, newY))
				{
					neighbors.Add((newX, newY));
				}
			}

			return neighbors;
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Gets spawn position for a player by index
		/// </summary>
		public (int X, int Y) GetSpawnPosition(int playerIndex)
		{
			if (playerIndex < 0 || playerIndex >= SpawnPositions.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(playerIndex),
					$"Player index must be between 0 and {SpawnPositions.Length - 1}");
			}

			return SpawnPositions[playerIndex];
		}

		/// <summary>
		/// Generates a simple ASCII representation of the map (for debugging)
		/// </summary>
		public string GetMapVisualization()
		{
			var sb = new System.Text.StringBuilder();

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					var cell = GetCell(x, y);

					if (cell.HasWall())
					{
						sb.Append(cell.Wall!.IsBreakable ? '▒' : '█');
					}
					else
					{
						sb.Append('·');
					}
				}
				sb.AppendLine();
			}

			return sb.ToString();
		}

		#endregion
	}
}