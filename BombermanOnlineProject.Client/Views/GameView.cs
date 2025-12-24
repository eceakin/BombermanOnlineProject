using BombermanOnlineProject.Client.Rendering;
using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Map;

namespace BombermanOnlineProject.Client.Views
{
	/// <summary>
	/// MVP Pattern - View Layer for Game Screen
	/// 
	/// Responsibilities:
	/// - Display game state
	/// - Show user notifications
	/// - Handle view lifecycle
	/// - Delegate rendering to ConsoleRenderer
	/// 
	/// Does NOT:
	/// - Contain game logic
	/// - Handle user input processing
	/// - Manage game state
	/// </summary>
	public class GameView : IGameView
	{
		#region Private Fields

		private readonly ConsoleRenderer _renderer;
		private readonly ThemeRenderer _themeRenderer;
		private bool _isVisible = false;

		#endregion

		#region Constructor

		public GameView(GameSettings.GameTheme theme = GameSettings.GameTheme.Forest)
		{
			_themeRenderer = new ThemeRenderer(theme);
			_renderer = new ConsoleRenderer(_themeRenderer);
		}

		#endregion

		#region IView Implementation - Lifecycle

		public void Show()
		{
			_isVisible = true;
			Console.Clear();
			Console.CursorVisible = false;
			_renderer.RenderControls();
		}

		public void Hide()
		{
			_isVisible = false;
			Console.CursorVisible = true;
		}

		public void Clear()
		{
			if (_isVisible)
			{
				Console.Clear();
			}
		}

		#endregion

		#region IView Implementation - Messages

		public void DisplayMessage(string message)
		{
			int currentLine = 25;
			Console.SetCursorPosition(0, currentLine);
			Console.WriteLine(new string(' ', Console.WindowWidth)); // Satırı temizle
			Console.SetCursorPosition(0, currentLine);
			Console.WriteLine($"> {message}");
		}

		public void DisplayError(string error)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"[ERROR] {error}");
			Console.ResetColor();
		}

		public void DisplaySuccess(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"[SUCCESS] {message}");
			Console.ResetColor();
		}

		public bool ConfirmAction(string message)
		{
			Console.Write($"{message} (Y/N): ");
			var key = Console.ReadKey();
			Console.WriteLine();
			return key.Key == ConsoleKey.Y;
		}

		public string GetUserInput(string prompt)
		{
			Console.Write($"{prompt}: ");
			return Console.ReadLine() ?? string.Empty;
		}

		#endregion

		#region IGameView Implementation - Rendering

		public void RenderMap(
			GameMap map,
			Dictionary<string, (int X, int Y, bool IsAlive)> players,
			Dictionary<string, (int X, int Y)> bombs,
			Dictionary<string, List<(int X, int Y)>> explosions)
		{
			if (!_isVisible) return;

			// Delegate to ConsoleRenderer
			_renderer.RenderMap(map, players, bombs, explosions);
		}

		public void DisplayPlayerStats(
			string playerId,
			int score,
			int kills,
			int deaths,
			float speed,
			int bombPower,
			int maxBombs,
			int activeBombs)
		{
			if (!_isVisible) return;

			// Delegate to ConsoleRenderer
			_renderer.RenderPlayerStats(
				playerId, score, kills, deaths,
				speed, bombPower, maxBombs, activeBombs);
		}

		public void DisplayGameInfo(
			string sessionId,
			int currentRound,
			int playerCount,
			string gameState,
			TimeSpan duration)
		{
			if (!_isVisible) return;

			// Delegate to ConsoleRenderer
			_renderer.RenderGameInfo(
				sessionId, currentRound, playerCount,
				gameState, duration);
		}

		public void DisplayControls()
		{
			if (!_isVisible) return;

			// Delegate to ConsoleRenderer
			_renderer.RenderControls();
		}

		#endregion

		#region IGameView Implementation - Game Events

		public void ShowGameStarted()
		{
			_renderer.RenderMessageBox(
				"GAME STARTED!",
				"Get ready to play!",
				ConsoleColor.Green);

			Thread.Sleep(2000);
			Show();
		}

		public void ShowGameEnded(string winnerId, Dictionary<string, int> finalScores)
		{
			_renderer.RenderGameOver(winnerId, finalScores);
		}

		public void ShowRoundWon(string winnerId, int roundNumber)
		{
			_renderer.RenderRoundWinner(winnerId, roundNumber);
			Thread.Sleep(2000);
		}

		public void ShowPlayerJoined(string playerId, string playerName)
		{
			DisplayMessage($"[+] {playerName} joined the game");
		}

		public void ShowPlayerLeft(string playerId)
		{
			DisplayMessage($"[-] Player {TruncateString(playerId, 8)} left the game");
		}

		public void ShowBombPlaced(string playerId, int x, int y)
		{
			// Optional: Could add visual feedback here
			// For now, bomb will be visible in map render
		}

		public void ShowExplosion(int centerX, int centerY, List<(int X, int Y)> affectedCells)
		{
			// Optional: Could add sound effect or animation here
			// For now, explosion will be visible in map render
		}

		public void ShowPowerUpCollected(string playerId, string powerUpType)
		{
			DisplaySuccess($"[POWER-UP] Player collected {powerUpType}");
		}

		public void ShowPlayerDied(string playerId)
		{
			DisplayError($"[DEATH] Player {TruncateString(playerId, 8)} died");
		}

		#endregion

		#region IGameView Implementation - Input

		public ConsoleKey WaitForInput()
		{
			if (Console.KeyAvailable)
			{
				return Console.ReadKey(true).Key;
			}
			return ConsoleKey.NoName;
		}

		public void UpdateGameState(object gameState)
		{
			// Placeholder for future state updates
			// Could be used for advanced rendering logic
		}

		#endregion

		#region Theme Management

		/// <summary>
		/// Changes the game theme
		/// </summary>
		public void SetTheme(GameSettings.GameTheme theme)
		{
			_themeRenderer.SetTheme(theme);
			DisplaySuccess($"Theme changed to: {theme}");
		}

		/// <summary>
		/// Changes theme by name
		/// </summary>
		public void SetTheme(string themeName)
		{
			if (_themeRenderer.SetThemeByName(themeName))
			{
				DisplaySuccess($"Theme changed to: {themeName}");
			}
			else
			{
				DisplayError($"Invalid theme: {themeName}");
			}
		}

		/// <summary>
		/// Shows available themes
		/// </summary>
		public void ShowAvailableThemes()
		{
			Console.WriteLine("\n═══ Available Themes ═══\n");
			foreach (var theme in ThemeRenderer.GetAvailableThemes())
			{
				Console.WriteLine($"- {theme}: {ThemeRenderer.GetThemeDescription(theme)}");
			}
			Console.WriteLine();
		}

		/// <summary>
		/// Displays theme preview
		/// </summary>
		public void PreviewTheme()
		{
			_themeRenderer.DisplayThemePreview();
		}

		#endregion

		#region Utility Methods

		private string TruncateString(string str, int maxLength)
		{
			if (string.IsNullOrEmpty(str)) return string.Empty;
			return str.Length <= maxLength ? str : str.Substring(0, maxLength);
		}

		#endregion
	}
}