using BombermanOnline.Server.Core.Map;
using BombermanOnlineProject.Server.Core.Map;
using BombermanOnlineProject.Server.Patterns.Creational.Factory;

// Factory test
var wallFactory = new WallFactory();
var breakableWall = wallFactory.Create("Breakable", 5, 5);
Console.WriteLine($"Created: {breakableWall}");

// Map generation test
var map = new GameMap();
Console.WriteLine("\nMap Visualization:");
Console.WriteLine(map.GetMapVisualization());

// Spawn positions
for (int i = 0; i < 2; i++)
{
	var (x, y) = map.GetSpawnPosition(i);
	Console.WriteLine($"Player {i + 1} spawn: ({x}, {y})");
}