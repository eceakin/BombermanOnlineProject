
using BombermanOnlineProject.Server.Core.Entities;
using BombermanOnlineProject.Server.Core.Entities.Enemies;
using BombermanOnlineProject.Server.Core.Map;
using BombermanOnlineProject.Server.Patterns.Creational.Factory;

Console.WriteLine("=== FASE 2 COMPLETE TEST ===\n");

// 1. Map Generation Test
Console.WriteLine("--- 1. Map Generation ---");
var map = new GameMap();
Console.WriteLine($"Map: {map.Width}x{map.Height}");
Console.WriteLine(map.GetMapVisualization());

// 2. PowerUp Factory Test
Console.WriteLine("\n--- 2. PowerUp Factory Test ---");
var powerUpFactory = new PowerUpFactory();
var speedBoost = powerUpFactory.Create("SpeedBoost", 5, 5);
var bombPower = powerUpFactory.Create("BombPower", 6, 6);
var bombCount = powerUpFactory.Create("BombCount", 7, 7);
Console.WriteLine(speedBoost);
Console.WriteLine(bombPower);
Console.WriteLine(bombCount);

var randomPowerUp = powerUpFactory.CreateRandom(8, 8);
Console.WriteLine($"Random: {randomPowerUp}");

// 3. Player Test
Console.WriteLine("\n--- 3. Player Test ---");
var (p1X, p1Y) = map.GetSpawnPosition(0);
var (p2X, p2Y) = map.GetSpawnPosition(1);

var player1 = new Player("Alice", 0, p1X, p1Y);
var player2 = new Player("Bob", 1, p2X, p2Y);

Console.WriteLine(player1);
Console.WriteLine(player2);

// Player collects power-up
player1.CollectPowerUp(speedBoost);
player1.CollectPowerUp(bombPower);
Console.WriteLine($"Player1 Speed: {player1.Speed}, BombPower: {player1.BombPower}");

// 4. Bomb Test
Console.WriteLine("\n--- 4. Bomb Test ---");
if (player1.CanPlaceBomb())
{
	player1.PlaceBomb();
	var bomb = new Bomb(player1.Id, player1.X, player1.Y, player1.BombPower);
	Console.WriteLine(bomb);
	Console.WriteLine($"Player1 Active Bombs: {player1.ActiveBombs}/{player1.MaxBombs}");

	// Simulate bomb explosion
	Thread.Sleep(100);
	bomb.Update(0.1f);
	Console.WriteLine($"Bomb remaining: {bomb.GetRemainingMilliseconds()}ms");
}

// 5. Explosion Test
Console.WriteLine("\n--- 5. Explosion Test ---");
var explosion = new Explosion(player1.Id, 5, 5, 3, map);
Console.WriteLine(explosion);
Console.WriteLine($"Affected cells: {explosion.AffectedCells.Count}");
foreach (var (x, y) in explosion.AffectedCells.Take(5))
{
	Console.WriteLine($"  - ({x}, {y})");
}

// 6. Enemy Factory Test
Console.WriteLine("\n--- 6. Enemy Factory Test ---");
var enemyFactory = new EnemyFactory();
var staticEnemy = enemyFactory.Create("Static", 7, 7);
var chasingEnemy = enemyFactory.Create("Chasing", 8, 8);

Console.WriteLine(staticEnemy);
Console.WriteLine(chasingEnemy);

// Test enemy movement
Console.WriteLine("\nEnemy Movement Test:");
var nextMove = staticEnemy.GetNextMove(map);
Console.WriteLine($"Static enemy next move: ({nextMove.X}, {nextMove.Y})");

if (chasingEnemy is ChasingEnemy ce)
{
	bool inRange = ce.IsPlayerInRange(player1.X, player1.Y);
	Console.WriteLine($"Player in range: {inRange}");

	if (inRange)
	{
		var chaseMove = ce.GetNextMove(map, player1.X, player1.Y);
		Console.WriteLine($"Chasing enemy move toward player: ({chaseMove.X}, {chaseMove.Y})");
	}
}

// 7. Random Factories Test
Console.WriteLine("\n--- 7. Random Factories Test ---");
var randomWall = new WallFactory().CreateRandom(10, 10);
var randomEnemy = enemyFactory.CreateRandom(11, 11);
var randomPowerUp2 = powerUpFactory.CreateWeightedRandom(12, 12);

Console.WriteLine($"Random Wall: {randomWall}");
Console.WriteLine($"Random Enemy: {randomEnemy}");
Console.WriteLine($"Random PowerUp: {randomPowerUp2}");

Console.WriteLine("\n=== ALL TESTS COMPLETED ===");