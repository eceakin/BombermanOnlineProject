using BombermanOnlineProject.Server.Core.Map;
using BombermanOnlineProject.Server.Core.Walls;
using System.Text;

namespace BombermanOnlineProject.Client.Rendering
{
	/// <summary>
	/// Handles all console-based rendering operations for the Bomberman game.
	/// 
	/// Responsibilities:
	/// - ASCII art rendering
	/// - Map visualization
	/// - Entity rendering (players, bombs, explosions)
	/// - UI element rendering (borders, stats, menus)
	/// - Thread-safe console operations
	/// 
	/// Design Principles:
	/// - Single Responsibility: Only handles rendering logic
	/// - Separation of Concerns: Theme-specific logic delegated to ThemeRenderer
	/// - Thread Safety: All console operations are synchronized
	/// </summary>
	public class ConsoleRenderer
	{
		#region Private Fields

		private readonly ThemeRenderer _themeRenderer;
		private readonly object _renderLock;
		private const int STATS_PANEL_WIDTH = 50;
		private const int MAP_PANEL_TOP = 6;

		#endregion

		#region Constructor

		public ConsoleRenderer(ThemeRenderer themeRenderer)
		{
			_themeRenderer = themeRenderer ?? throw new ArgumentNullException(nameof(themeRenderer));
			_renderLock = new object();
		}

		#endregion

		#region Public Rendering Methods

		/// <summary>
		/// Renders the complete game map with all entities
		/// </summary>
		public void RenderMap(
			GameMap map,
			Dictionary<string, (int X, int Y, bool IsAlive)> players,
			Dictionary<string, (int X, int Y)> bombs,
			Dictionary<string, List<(int X, int Y)>> explosions)
		{
			lock (_renderLock)
			{
				// Move cursor to map area (don't clear entire screen)
				Console.SetCursorPosition(0, MAP_PANEL_TOP);

				// Build explosion lookup for fast checks
				var explosionCells = BuildExplosionLookup(explosions);
				var bombPositions = bombs.Values.Select(b => (b.X, b.Y)).ToHashSet();
				var playerPositions = BuildPlayerLookup(players);

				// Render map border (top)
				RenderMapBorder(map, isTop: true);

				// Render map content row by row
				for (int y = 0; y < map.Height; y++)
				{
					RenderMapRow(map, y, playerPositions, bombPositions, explosionCells);
				}

				// Render map border (bottom)
				RenderMapBorder(map, isTop: false);

				Console.ResetColor();
			}
		}

		/// <summary>
		/// Renders player statistics panel
		/// </summary>
		public void RenderPlayerStats(
			string playerId,
			int score,
			int kills,
			int deaths,
			float speed,
			int bombPower,
			int maxBombs,
			int activeBombs)
		{
			lock (_renderLock)
			{
				Console.SetCursorPosition(0, 0);

				var theme = _themeRenderer.CurrentTheme;
				Console.ForegroundColor = _themeRenderer.GetThemeColor(theme, "PlayerPanel");

				Console.WriteLine("╔═══════════════════════════════════════════════╗");
				Console.WriteLine($"║ Player: {TruncateString(playerId, 8).PadRight(35)} ║");
				Console.WriteLine($"║ Score: {score.ToString().PadRight(5)} | Kills: {kills.ToString().PadRight(3)} | Deaths: {deaths.ToString().PadRight(3)}   ║");
				Console.WriteLine($"║ Speed: {speed:F1} | Power: {bombPower} | Bombs: {activeBombs}/{maxBombs}     ║");
				Console.WriteLine("╚═══════════════════════════════════════════════╝");

				Console.ResetColor();
			}
		}

		/// <summary>
		/// Renders game information panel
		/// </summary>
		public void RenderGameInfo(
			string sessionId,
			int currentRound,
			int playerCount,
			string gameState,
			TimeSpan duration)
		{
			lock (_renderLock)
			{
				Console.SetCursorPosition(STATS_PANEL_WIDTH, 0);

				var theme = _themeRenderer.CurrentTheme;
				Console.ForegroundColor = _themeRenderer.GetThemeColor(theme, "InfoPanel");

				Console.WriteLine("╔══════════════════════════════╗");
				Console.WriteLine($"║ Session: {TruncateString(sessionId, 8).PadRight(16)} ║");
				Console.WriteLine($"║ Round: {currentRound.ToString().PadRight(3)} | Players: {playerCount}      ║");
				Console.WriteLine($"║ State: {gameState.PadRight(20)} ║");
				Console.WriteLine($"║ Time: {duration.ToString(@"mm\:ss").PadRight(21)} ║");
				Console.WriteLine("╚══════════════════════════════╝");

				Console.ResetColor();
			}
		}

		/// <summary>
		/// Renders control hints at bottom of screen
		/// </summary>
		public void RenderControls()
		{
			lock (_renderLock)
			{
				int controlsTop = Console.WindowHeight - 4;
				if (controlsTop < 0) controlsTop = 20; // Fallback

				Console.SetCursorPosition(0, controlsTop);

				var theme = _themeRenderer.CurrentTheme;
				Console.ForegroundColor = _themeRenderer.GetThemeColor(theme, "Controls");

				Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
				Console.WriteLine("║ Controls: ↑↓←→ Move | SPACE Bomb | ESC Quit | P Pause       ║");
				Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");

				Console.ResetColor();
			}
		}

		/// <summary>
		/// Displays a centered message box
		/// </summary>
		public void RenderMessageBox(string title, string message, ConsoleColor color = ConsoleColor.White)
		{
			lock (_renderLock)
			{
				Console.Clear();
				Console.ForegroundColor = color;

				int centerY = Console.WindowHeight / 2 - 3;
				int centerX = (Console.WindowWidth - 40) / 2;

				Console.SetCursorPosition(centerX, centerY);
				Console.WriteLine("╔══════════════════════════════════════╗");

				Console.SetCursorPosition(centerX, centerY + 1);
				Console.WriteLine($"║ {CenterText(title, 36)} ║");

				Console.SetCursorPosition(centerX, centerY + 2);
				Console.WriteLine("╠══════════════════════════════════════╣");

				Console.SetCursorPosition(centerX, centerY + 3);
				Console.WriteLine($"║ {CenterText(message, 36)} ║");

				Console.SetCursorPosition(centerX, centerY + 4);
				Console.WriteLine("╚══════════════════════════════════════╝");

				Console.ResetColor();
			}
		}

		/// <summary>
		/// Displays game over screen with final scores
		/// </summary>
		public void RenderGameOver(string winnerId, Dictionary<string, int> finalScores)
		{
			lock (_renderLock)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;

				int startY = Console.WindowHeight / 2 - 5;

				Console.SetCursorPosition(0, startY);
				Console.WriteLine();
				Console.WriteLine("  ╔═══════════════════════════════════╗");
				Console.WriteLine("  ║                                   ║");
				Console.WriteLine("  ║         GAME OVER!                ║");
				Console.WriteLine("  ║                                   ║");
				Console.WriteLine("  ╚═══════════════════════════════════╝");
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"  Winner: {TruncateString(winnerId, 20)}");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("\n  Final Scores:");

				foreach (var score in finalScores.OrderByDescending(s => s.Value))
				{
					Console.WriteLine($"    {TruncateString(score.Key, 15)}: {score.Value}");
				}

				Console.ResetColor();
				Console.WriteLine("\n  Press any key to continue...");
			}
		}

		/// <summary>
		/// Renders round winner announcement
		/// </summary>
		public void RenderRoundWinner(string winnerId, int roundNumber)
		{
			lock (_renderLock)
			{
				int centerY = Console.WindowHeight / 2;
				Console.SetCursorPosition(0, centerY);

				Console.ForegroundColor = ConsoleColor.Green;
				string message = $"ROUND {roundNumber} WON BY {TruncateString(winnerId, 10)}";
				Console.WriteLine($"\n  ═══ {message} ═══\n");
				Console.ResetColor();
			}
		}

		#endregion

		#region Private Rendering Helpers

		/// <summary>
		/// Renders map border (top or bottom)
		/// </summary>
		private void RenderMapBorder(GameMap map, bool isTop)
		{
			var sb = new StringBuilder();
			sb.Append(isTop ? "┌" : "└");
			sb.Append(new string('─', map.Width * 2));
			sb.Append(isTop ? "┐" : "┘");
			Console.WriteLine(sb.ToString());
		}

		/// <summary>
		/// Renders a single row of the map
		/// </summary>
		private void RenderMapRow(
			GameMap map,
			int y,
			Dictionary<(int, int), (string Id, bool IsAlive)> playerPositions,
			HashSet<(int, int)> bombPositions,
			HashSet<(int, int)> explosionCells)
		{
			var sb = new StringBuilder();
			sb.Append("│");

			for (int x = 0; x < map.Width; x++)
			{
				var cell = map.GetCell(x, y);
				char displayChar;
				ConsoleColor color;

				// Priority: Explosion > Player > Bomb > PowerUp > Wall > Empty
				if (explosionCells.Contains((x, y)))
				{
					displayChar = '※';
					color = ConsoleColor.Yellow;
				}
				else if (playerPositions.ContainsKey((x, y)))
				{
					var (playerId, isAlive) = playerPositions[(x, y)];
					displayChar = isAlive ? 'P' : 'X';
					color = isAlive ? ConsoleColor.Cyan : ConsoleColor.DarkRed;
				}
				else if (bombPositions.Contains((x, y)))
				{
					displayChar = '●';
					color = ConsoleColor.Red;
				}
				else if (cell.HasPowerUp())
				{
					displayChar = cell.PowerUp!.DisplayChar;
					color = ConsoleColor.Magenta;
				}
				else if (cell.HasWall())
				{
					displayChar = _themeRenderer.GetWallChar(cell.Wall!, map.Theme);
					color = _themeRenderer.GetWallColor(cell.Wall!, map.Theme);
				}
				else
				{
					displayChar = '·';
					color = ConsoleColor.DarkGray;
				}

				// Write character with color
				Console.ForegroundColor = color;
				sb.Append(displayChar);
				sb.Append(' ');
			}

			Console.ResetColor();
			sb.Append("│");
			Console.WriteLine(sb.ToString());
		}

		/// <summary>
		/// Builds explosion cell lookup for fast collision checks
		/// </summary>
		private HashSet<(int, int)> BuildExplosionLookup(Dictionary<string, List<(int X, int Y)>> explosions)
		{
			var lookup = new HashSet<(int, int)>();
			foreach (var explosion in explosions.Values)
			{
				foreach (var cell in explosion)
				{
					lookup.Add((cell.X, cell.Y));
				}
			}
			return lookup;
		}

		/// <summary>
		/// Builds player position lookup
		/// </summary>
		private Dictionary<(int, int), (string, bool)> BuildPlayerLookup(
			Dictionary<string, (int X, int Y, bool IsAlive)> players)
		{
			var lookup = new Dictionary<(int, int), (string, bool)>();
			foreach (var kvp in players)
			{
				lookup[(kvp.Value.X, kvp.Value.Y)] = (kvp.Key, kvp.Value.IsAlive);
			}
			return lookup;
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Truncates string to specified length
		/// </summary>
		private string TruncateString(string str, int maxLength)
		{
			if (string.IsNullOrEmpty(str)) return string.Empty;
			return str.Length <= maxLength ? str : str.Substring(0, maxLength);
		}

		/// <summary>
		/// Centers text within specified width
		/// </summary>
		private string CenterText(string text, int width)
		{
			if (text.Length >= width) return text.Substring(0, width);
			int padding = (width - text.Length) / 2;
			return text.PadLeft(padding + text.Length).PadRight(width);
		}

		#endregion
	}
}