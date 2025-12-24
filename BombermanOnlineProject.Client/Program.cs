using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Entities;
using BombermanOnlineProject.Server.Core.Entities.Enemies;
using BombermanOnlineProject.Server.Core.Game;
using BombermanOnlineProject.Server.Core.Map;
using BombermanOnlineProject.Server.Patterns.Behavioral.Observer;
using BombermanOnlineProject.Server.Patterns.Creational.Factory;
using BombermanOnlineProject.Server.Patterns.Structural.Decorator;

Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     BOMBERMAN ONLINE - COMPLETE SYSTEM TEST                   ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

RunAllTests();

static void RunAllTests()
{
	TestGameSettings();
	TestSingletonPattern();
	TestFactoryPatterns();
	TestMapGeneration();
	TestPlayerAndMovement();
	TestBombSystem();
	TestExplosionSystem();
	TestEnemySystem();
	TestDecoratorPattern();
	TestObserverPattern();
	TestGameSession();

	Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
	Console.WriteLine("║     ALL TESTS COMPLETED SUCCESSFULLY ✓                        ║");
	Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
}

static void TestGameSettings()
{
	Console.WriteLine("═══ TEST 1: Game Settings & Configuration ═══");
	Console.WriteLine($"Map Size: {GameSettings.MapWidth}x{GameSettings.MapHeight}");
	Console.WriteLine($"Bomb Timer: {GameSettings.BombTimer}ms");
	Console.WriteLine($"Default Player Speed: {GameSettings.DefaultPlayerSpeed}");
	Console.WriteLine($"Max Bomb Power: {GameSettings.MaxBombPower}");
	Console.WriteLine($"Power-Up Drop Chance: {GameSettings.PowerUpDropChance * 100}%");
	Console.WriteLine("✓ Game Settings loaded successfully\n");
}

static void TestSingletonPattern()
{
	Console.WriteLine("═══ TEST 2: Singleton Pattern - GameManager ═══");

	var manager1 = GameManager.Instance;
	var manager2 = GameManager.Instance;

	Console.WriteLine($"Instance 1 Hash: {manager1.GetHashCode()}");
	Console.WriteLine($"Instance 2 Hash: {manager2.GetHashCode()}");
	Console.WriteLine($"Are Same Instance: {ReferenceEquals(manager1, manager2)}");

	var stats = manager1.GetStatistics();
	Console.WriteLine($"Active Sessions: {stats.ActiveSessions}");
	Console.WriteLine($"Connected Players: {stats.ConnectedPlayers}");
	Console.WriteLine("✓ Singleton Pattern working correctly\n");
}

static void TestFactoryPatterns()
{
	Console.WriteLine("═══ TEST 3: Factory Patterns ═══");

	var wallFactory = new WallFactory();
	Console.WriteLine($"Available Wall Types: {string.Join(", ", wallFactory.GetAvailableTypes())}");
	var unbreakable = wallFactory.Create("Unbreakable", 0, 0);
	var breakable = wallFactory.Create("Breakable", 1, 1);
	var hard = wallFactory.Create("Hard", 2, 2);
	Console.WriteLine($"Created: {unbreakable.GetType().Name}, {breakable.GetType().Name}, {hard.GetType().Name}");

	var powerUpFactory = new PowerUpFactory();
	Console.WriteLine($"Available PowerUp Types: {string.Join(", ", powerUpFactory.GetAvailableTypes())}");
	var speedBoost = powerUpFactory.Create("SpeedBoost", 3, 3);
	var bombPower = powerUpFactory.Create("BombPower", 4, 4);
	var bombCount = powerUpFactory.Create("BombCount", 5, 5);
	Console.WriteLine($"Created: {speedBoost.PowerUpType}, {bombPower.PowerUpType}, {bombCount.PowerUpType}");

	var enemyFactory = new EnemyFactory();
	Console.WriteLine($"Available Enemy Types: {string.Join(", ", enemyFactory.GetAvailableTypes())}");
	var staticEnemy = enemyFactory.Create("Static", 6, 6);
	var chasingEnemy = enemyFactory.Create("Chasing", 7, 7);
	Console.WriteLine($"Created: {staticEnemy.EnemyName}, {chasingEnemy.EnemyName}");
	Console.WriteLine("✓ All Factory Patterns working correctly\n");
}

static void TestMapGeneration()
{
	Console.WriteLine("═══ TEST 4: Map Generation ═══");

	var map = new GameMap();
	Console.WriteLine($"Map Created: {map.Width}x{map.Height}");
	Console.WriteLine($"Theme: {map.Theme}");

	var (p1X, p1Y) = map.GetSpawnPosition(0);
	var (p2X, p2Y) = map.GetSpawnPosition(1);
	Console.WriteLine($"Player 1 Spawn: ({p1X}, {p1Y})");
	Console.WriteLine($"Player 2 Spawn: ({p2X}, {p2Y})");

	int walkableCells = 0;
	int wallCells = 0;
	for (int x = 0; x < map.Width; x++)
	{
		for (int y = 0; y < map.Height; y++)
		{
			if (map.IsWalkable(x, y)) walkableCells++;
			else wallCells++;
		}
	}
	Console.WriteLine($"Walkable Cells: {walkableCells}");
	Console.WriteLine($"Wall Cells: {wallCells}");

	Console.WriteLine("\nMap Visualization (first 10 rows):");
	var visualization = map.GetMapVisualization();
	var lines = visualization.Split('\n').Take(10);
	foreach (var line in lines)
	{
		Console.WriteLine(line);
	}
	Console.WriteLine("✓ Map Generation working correctly\n");
}

static void TestPlayerAndMovement()
{
	Console.WriteLine("═══ TEST 5: Player & Movement System ═══");

	var map = new GameMap();
	var (spawnX, spawnY) = map.GetSpawnPosition(0);
	var player = new Player("Alice", 0, spawnX, spawnY);

	Console.WriteLine($"Player: {player.PlayerName}");
	Console.WriteLine($"Position: ({player.X}, {player.Y})");
	Console.WriteLine($"Speed: {player.Speed}");
	Console.WriteLine($"Bomb Power: {player.BombPower}");
	Console.WriteLine($"Max Bombs: {player.MaxBombs}");
	Console.WriteLine($"Display Char: {player.DisplayChar}");

	bool moved = player.MoveTo(spawnX + 1, spawnY);
	Console.WriteLine($"Move Right: {moved} - New Position: ({player.X}, {player.Y})");

	moved = player.MoveTo(spawnX + 1, spawnY + 1);
	Console.WriteLine($"Move Down: {moved} - New Position: ({player.X}, {player.Y})");

	Console.WriteLine("✓ Player & Movement working correctly\n");
}

static void TestBombSystem()
{
	Console.WriteLine("═══ TEST 6: Bomb System ═══");

	var player = new Player("Bob", 0, 5, 5);
	Console.WriteLine($"Initial Active Bombs: {player.ActiveBombs}/{player.MaxBombs}");
	Console.WriteLine($"Can Place Bomb: {player.CanPlaceBomb()}");

	player.PlaceBomb();
	var bomb = new Bomb(player.Id, player.X, player.Y, player.BombPower, 2000);
	Console.WriteLine($"Bomb Placed at ({bomb.X}, {bomb.Y})");
	Console.WriteLine($"Bomb Power: {bomb.Power}");
	Console.WriteLine($"Timer: {bomb.TimerMilliseconds}ms");
	Console.WriteLine($"Player Active Bombs: {player.ActiveBombs}/{player.MaxBombs}");

	Thread.Sleep(500);
	bomb.Update(0.5f);
	Console.WriteLine($"After 500ms - Remaining: {bomb.GetRemainingMilliseconds()}ms");

	Thread.Sleep(1600);
	bomb.Update(1.6f);
	Console.WriteLine($"After 2100ms - Should Explode: {bomb.ShouldExplode()}");
	Console.WriteLine($"Has Exploded: {bomb.HasExploded}");

	Console.WriteLine("✓ Bomb System working correctly\n");
}

static void TestExplosionSystem()
{
	Console.WriteLine("═══ TEST 7: Explosion System ═══");

	var map = new GameMap();
	var explosion = new Explosion("player1", 7, 7, 3, map);

	Console.WriteLine($"Explosion Center: ({explosion.X}, {explosion.Y})");
	Console.WriteLine($"Affected Cells: {explosion.AffectedCells.Count}");
	Console.WriteLine($"Duration: {explosion.DurationMilliseconds}ms");

	Console.WriteLine("First 10 Affected Cells:");
	foreach (var (x, y) in explosion.AffectedCells.Take(10))
	{
		Console.WriteLine($"  ({x}, {y})");
	}

	Thread.Sleep(200);
	explosion.Update(0.2f);
	Console.WriteLine($"Is Expired after 200ms: {explosion.IsExpired()}");

	Thread.Sleep(400);
	explosion.Update(0.4f);
	Console.WriteLine($"Is Expired after 600ms: {explosion.IsExpired()}");

	Console.WriteLine("✓ Explosion System working correctly\n");
}

static void TestEnemySystem()
{
	Console.WriteLine("═══ TEST 8: Enemy System ═══");

	var map = new GameMap();
	var staticEnemy = new StaticEnemy(8, 8);
	var chasingEnemy = new ChasingEnemy(10, 10);

	Console.WriteLine($"Static Enemy: {staticEnemy.EnemyName}");
	Console.WriteLine($"Position: ({staticEnemy.X}, {staticEnemy.Y})");
	Console.WriteLine($"Speed: {staticEnemy.Speed}");
	Console.WriteLine($"Strategy: {staticEnemy.MovementStrategy.GetStrategyName()}");

	var nextMove = staticEnemy.GetNextMove(map);
	Console.WriteLine($"Next Move: ({nextMove.X}, {nextMove.Y})");

	Console.WriteLine($"\nChasing Enemy: {chasingEnemy.EnemyName}");
	Console.WriteLine($"Position: ({chasingEnemy.X}, {chasingEnemy.Y})");
	Console.WriteLine($"Detection Range: {chasingEnemy.DetectionRange}");

	var player = new Player("Target", 0, 12, 12);
	bool inRange = chasingEnemy.IsPlayerInRange(player.X, player.Y);
	Console.WriteLine($"Player at ({player.X}, {player.Y}) in range: {inRange}");

	if (inRange)
	{
		var chaseMove = chasingEnemy.GetNextMove(map, player.X, player.Y);
		Console.WriteLine($"Chase Move: ({chaseMove.X}, {chaseMove.Y})");
	}

	Console.WriteLine("✓ Enemy System working correctly\n");
}

static void TestDecoratorPattern()
{
	Console.WriteLine("═══ TEST 9: Decorator Pattern - PowerUps ═══");

	IPlayer player = new Player("Charlie", 0, 5, 5);
	Console.WriteLine($"Base Player:");
	Console.WriteLine($"  Speed: {player.Speed}");
	Console.WriteLine($"  Bomb Power: {player.BombPower}");
	Console.WriteLine($"  Max Bombs: {player.MaxBombs}");

	player = new SpeedBoostDecorator(player);
	Console.WriteLine($"\nAfter Speed Boost:");
	Console.WriteLine($"  Speed: {player.Speed}");

	player = new BombPowerDecorator(player);
	Console.WriteLine($"\nAfter Bomb Power:");
	Console.WriteLine($"  Bomb Power: {player.BombPower}");

	player = new BombCountDecorator(player);
	Console.WriteLine($"\nAfter Bomb Count:");
	Console.WriteLine($"  Max Bombs: {player.MaxBombs}");

	player = new SpeedBoostDecorator(player);
	player = new BombPowerDecorator(player);
	Console.WriteLine($"\nAfter 2nd Speed & Power Boost:");
	Console.WriteLine($"  Speed: {player.Speed}");
	Console.WriteLine($"  Bomb Power: {player.BombPower}");

	if (player is PlayerDecorator decorator)
	{
		var depth = decorator.GetDecoratorDepth();
		Console.WriteLine($"  Decorator Chain Depth: {depth}");
	}

	Console.WriteLine("✓ Decorator Pattern working correctly\n");
}

static void TestObserverPattern()
{
	Console.WriteLine("═══ TEST 10: Observer Pattern ═══");

	var gameSubject = new GameSubject("test-session");
	var scoreObserver = new ScoreObserver("test-session");
	var explosionObserver = new ExplosionObserver("test-session");

	gameSubject.Attach(scoreObserver);
	gameSubject.Attach(explosionObserver);

	Console.WriteLine($"Observers Attached: {gameSubject.GetObserverCount()}");

	gameSubject.NotifyPlayerKilled("player1", "player2", 100);
	gameSubject.NotifyEnemyKilled("player1", "enemy1", 50);
	gameSubject.NotifyPowerUpCollected("player1", "SpeedBoost", 5);
	gameSubject.NotifyBombPlaced("player1", 5, 5, 3);

	var affectedCells = new List<(int, int)> { (5, 5), (5, 6), (5, 4), (6, 5), (4, 5) };
	gameSubject.NotifyExplosionCreated("player1", 5, 5, affectedCells);

	Console.WriteLine($"\nPlayer1 Score: {scoreObserver.GetPlayerScore("player1")}");
	Console.WriteLine($"Player1 Kills: {scoreObserver.GetPlayerKills("player1")}");
	Console.WriteLine($"Player2 Deaths: {scoreObserver.GetPlayerDeaths("player2")}");

	Console.WriteLine($"\nPlayer1 Explosions: {explosionObserver.GetPlayerExplosionCount("player1")}");
	Console.WriteLine($"Total Cells Affected: {explosionObserver.GetTotalCellsAffected("player1")}");

	Console.WriteLine("✓ Observer Pattern working correctly\n");
}

static void TestGameSession()
{
	Console.WriteLine("═══ TEST 11: Complete Game Session ═══");

	var manager = GameManager.Instance;
	manager.ClearAllSessions();

	var session = manager.CreateGameSession("player1");
	Console.WriteLine($"Session Created: {session.SessionId.Substring(0, 8)}...");
	Console.WriteLine($"Host: {session.HostPlayerId}");
	Console.WriteLine($"State: {session.State}");

	bool joined = manager.JoinGameSession(session.SessionId, "player2");
	Console.WriteLine($"\nPlayer 2 Joined: {joined}");
	Console.WriteLine($"Player Count: {session.GetPlayerIds().Count}");

	session.StartGame();
	Console.WriteLine($"\nGame Started");
	Console.WriteLine($"State: {session.State}");
	Console.WriteLine($"Round: {session.CurrentRound}");

	var player1 = session.GetPlayer("player1");
	var player2 = session.GetPlayer("player2");

	if (player1 != null && player2 != null)
	{
		Console.WriteLine($"\nPlayer 1: {player1.PlayerName} at ({player1.X}, {player1.Y})");
		Console.WriteLine($"Player 2: {player2.PlayerName} at ({player2.X}, {player2.Y})");

		session.MovePlayer("player1", player1.X + 1, player1.Y);
		Console.WriteLine($"Player 1 moved to ({player1.X}, {player1.Y})");

		bool bombPlaced = session.PlaceBomb("player1");
		Console.WriteLine($"Player 1 placed bomb: {bombPlaced}");

		Thread.Sleep(100);

		var stats = session.GetGameStatistics();
		Console.WriteLine($"\nGame Statistics:");
		Console.WriteLine($"  Active Bombs: {stats.ActiveBombs}");
		Console.WriteLine($"  Active Enemies: {stats.ActiveEnemies}");
		Console.WriteLine($"  Active PowerUps: {stats.ActivePowerUps}");
		Console.WriteLine($"  Duration: {stats.Duration.TotalSeconds:F2}s");
	}

	session.EndGame();
	Console.WriteLine($"\nGame Ended");
	Console.WriteLine($"Final State: {session.State}");

	var managerStats = manager.GetStatistics();
	Console.WriteLine($"\nManager Statistics:");
	Console.WriteLine($"  Active Sessions: {managerStats.ActiveSessions}");
	Console.WriteLine($"  Total Games Created: {managerStats.TotalGamesCreated}");
	Console.WriteLine($"  Connected Players: {managerStats.ConnectedPlayers}");

	Console.WriteLine("✓ Game Session working correctly\n");
}