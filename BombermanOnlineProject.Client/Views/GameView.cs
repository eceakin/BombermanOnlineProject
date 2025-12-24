using BombermanOnlineProject.Server.Core.Map;
using System.Text;

namespace BombermanOnlineProject.Client.Views
{
	public class GameView : IGameView
	{
		private readonly object _renderLock = new object();
		private bool _isVisible = false;

		public void Show()
		{
			_isVisible = true;
			Console.Clear();
			Console.CursorVisible = false;
			DisplayControls();
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

		public void DisplayMessage(string message)
		{
			lock (_renderLock)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(message);
				Console.ResetColor();
			}
		}

		public void DisplayError(string error)
		{
			lock (_renderLock)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"[ERROR] {error}");
				Console.ResetColor();
			}
		}

		public void DisplaySuccess(string message)
		{
			lock (_renderLock)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"[SUCCESS] {message}");
				Console.ResetColor();
			}
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

		public void RenderMap(GameMap map, Dictionary<string, (int X, int Y, bool IsAlive)> players,
							 Dictionary<string, (int X, int Y)> bombs,
							 Dictionary<string, List<(int X, int Y)>> explosions)
		{
			lock (_renderLock)
			{
				if (!_isVisible) return;

				Console.SetCursorPosition(0, 5);

				var explosionCells = new HashSet<(int, int)>();
				foreach (var explosion in explosions.Values)
				{
					foreach (var cell in explosion)
					{
						explosionCells.Add((cell.X, cell.Y));
					}
				}

				var bombPositions = bombs.Values.Select(b => (b.X, b.Y)).ToHashSet();
				var playerPositions = new Dictionary<(int, int), (string Id, bool IsAlive)>();

				foreach (var kvp in players)
				{
					playerPositions[(kvp.Value.X, kvp.Value.Y)] = (kvp.Key, kvp.Value.IsAlive);
				}

				var sb = new StringBuilder();
				sb.AppendLine("┌" + new string('─', map.Width * 2) + "┐");

				for (int y = 0; y < map.Height; y++)
				{
					sb.Append("│");
					for (int x = 0; x < map.Width; x++)
					{
						var cell = map.GetCell(x, y);
						char displayChar = ' ';
						ConsoleColor color = ConsoleColor.Gray;

						if (explosionCells.Contains((x, y)))
						{
							displayChar = '※';
							color = ConsoleColor.Yellow;
						}
						else if (playerPositions.ContainsKey((x, y)))
						{
							var (playerId, isAlive) = playerPositions[(x, y)];
							displayChar = playerId.Contains("player1") ? 'P' : 'Q';
							color = isAlive ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
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
							displayChar = cell.Wall!.DisplayChar;
							color = cell.Wall.IsBreakable ? ConsoleColor.DarkYellow : ConsoleColor.White;
						}
						else
						{
							displayChar = '·';
							color = ConsoleColor.DarkGray;
						}

						Console.ForegroundColor = color;
						sb.Append(displayChar);
						sb.Append(' ');
					}
					Console.ResetColor();
					sb.AppendLine("│");
				}

				sb.AppendLine("└" + new string('─', map.Width * 2) + "┘");
				Console.Write(sb.ToString());
				Console.ResetColor();
			}
		}

		public void DisplayPlayerStats(string playerId, int score, int kills, int deaths,
									  float speed, int bombPower, int maxBombs, int activeBombs)
		{
			lock (_renderLock)
			{
				Console.SetCursorPosition(0, 0);
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine($"╔═══════════════════════════════════════════════╗");
				Console.WriteLine($"║ Player: {playerId.Substring(0, Math.Min(8, playerId.Length)).PadRight(35)} ║");
				Console.WriteLine($"║ Score: {score.ToString().PadRight(5)} | Kills: {kills.ToString().PadRight(3)} | Deaths: {deaths.ToString().PadRight(3)}   ║");
				Console.WriteLine($"║ Speed: {speed:F1} | Power: {bombPower} | Bombs: {activeBombs}/{maxBombs}     ║");
				Console.WriteLine($"╚═══════════════════════════════════════════════╝");
				Console.ResetColor();
			}
		}

		public void DisplayGameInfo(string sessionId, int currentRound, int playerCount,
								   string gameState, TimeSpan duration)
		{
			lock (_renderLock)
			{
				Console.SetCursorPosition(50, 0);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"╔══════════════════════════════╗");
				Console.WriteLine($"║ Session: {sessionId.Substring(0, Math.Min(8, sessionId.Length)).PadRight(16)} ║");
				Console.WriteLine($"║ Round: {currentRound.ToString().PadRight(3)} | Players: {playerCount}      ║");
				Console.WriteLine($"║ State: {gameState.PadRight(20)} ║");
				Console.WriteLine($"║ Time: {duration.ToString(@"mm\:ss").PadRight(21)} ║");
				Console.WriteLine($"╚══════════════════════════════╝");
				Console.ResetColor();
			}
		}

		public void DisplayControls()
		{
			lock (_renderLock)
			{
				Console.SetCursorPosition(0, Console.WindowHeight - 4);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
				Console.WriteLine("║ Controls: ↑↓←→ Move | SPACE Bomb | ESC Quit | P Pause       ║");
				Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
				Console.ResetColor();
			}
		}

		public void ShowGameStarted()
		{
			lock (_renderLock)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\n\n");
				Console.WriteLine("  ╔═══════════════════════════════════╗");
				Console.WriteLine("  ║                                   ║");
				Console.WriteLine("  ║         GAME STARTED!             ║");
				Console.WriteLine("  ║                                   ║");
				Console.WriteLine("  ╚═══════════════════════════════════╝");
				Console.ResetColor();
				Thread.Sleep(2000);
				Show();
			}
		}

		public void ShowGameEnded(string winnerId, Dictionary<string, int> finalScores)
		{
			lock (_renderLock)
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("\n\n");
				Console.WriteLine("  ╔═══════════════════════════════════╗");
				Console.WriteLine("  ║                                   ║");
				Console.WriteLine("  ║         GAME ENDED!               ║");
				Console.WriteLine("  ║                                   ║");
				Console.WriteLine("  ╚═══════════════════════════════════╝");
				Console.WriteLine();
				Console.WriteLine($"  Winner: {winnerId}");
				Console.WriteLine("\n  Final Scores:");
				foreach (var score in finalScores.OrderByDescending(s => s.Value))
				{
					Console.WriteLine($"    {score.Key}: {score.Value}");
				}
				Console.ResetColor();
				Console.WriteLine("\n  Press any key to continue...");
				Console.ReadKey();
			}
		}

		public void ShowRoundWon(string winnerId, int roundNumber)
		{
			lock (_renderLock)
			{
				var currentTop = Console.CursorTop;
				Console.SetCursorPosition(0, Console.WindowHeight / 2);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"\n  ═══ ROUND {roundNumber} WON BY {winnerId} ═══\n");
				Console.ResetColor();
				Thread.Sleep(2000);
			}
		}

		public void ShowPlayerJoined(string playerId, string playerName)
		{
			DisplayMessage($"[+] {playerName} joined the game");
		}

		public void ShowPlayerLeft(string playerId)
		{
			DisplayMessage($"[-] Player {playerId.Substring(0, Math.Min(8, playerId.Length))} left the game");
		}

		public void ShowBombPlaced(string playerId, int x, int y)
		{
			DisplayMessage($"[BOMB] Player placed bomb at ({x}, {y})");
		}

		public void ShowExplosion(int centerX, int centerY, List<(int X, int Y)> affectedCells)
		{
			DisplayMessage($"[EXPLOSION] Center: ({centerX}, {centerY}), Cells: {affectedCells.Count}");
		}

		public void ShowPowerUpCollected(string playerId, string powerUpType)
		{
			DisplaySuccess($"[POWER-UP] Player collected {powerUpType}");
		}

		public void ShowPlayerDied(string playerId)
		{
			DisplayError($"[DEATH] Player {playerId.Substring(0, Math.Min(8, playerId.Length))} died");
		}

		public void UpdateGameState(object gameState)
		{
		}

		public ConsoleKey WaitForInput()
		{
			if (Console.KeyAvailable)
			{
				return Console.ReadKey(true).Key;
			}
			return ConsoleKey.NoName;
		}
	}
}