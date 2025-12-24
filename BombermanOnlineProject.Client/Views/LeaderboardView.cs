namespace BombermanOnlineProject.Client.Views
{
	public class LeaderboardView : ILeaderboardView
	{
		private bool _isVisible = false;

		public void Show()
		{
			_isVisible = true;
			Console.Clear();
			Console.CursorVisible = true;
			DisplayHeader();
		}

		public void Hide()
		{
			_isVisible = false;
		}

		public void Clear()
		{
			if (_isVisible)
			{
				Console.Clear();
				DisplayHeader();
			}
		}

		public void DisplayMessage(string message)
		{
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine($"\n{message}");
			Console.ResetColor();
		}

		public void DisplayError(string error)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"\n[ERROR] {error}");
			Console.ResetColor();
		}

		public void DisplaySuccess(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"\n[SUCCESS] {message}");
			Console.ResetColor();
		}

		public bool ConfirmAction(string message)
		{
			Console.Write($"\n{message} (Y/N): ");
			var key = Console.ReadKey();
			Console.WriteLine();
			return key.Key == ConsoleKey.Y;
		}

		public string GetUserInput(string prompt)
		{
			Console.Write($"\n{prompt}: ");
			return Console.ReadLine() ?? string.Empty;
		}

		private void DisplayHeader()
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("║           BOMBERMAN ONLINE - LEADERBOARD                      ║");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
			Console.ResetColor();
			Console.WriteLine();
		}

		public void DisplayTopPlayersByScore(List<PlayerRankInfo> players)
		{
			Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("═══ TOP PLAYERS BY SCORE ═══\n");
			Console.ResetColor();

			DisplayPlayerTable(players, "Score");
		}

		public void DisplayTopPlayersByWins(List<PlayerRankInfo> players)
		{
			Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("═══ TOP PLAYERS BY WINS ═══\n");
			Console.ResetColor();

			DisplayPlayerTable(players, "Wins");
		}

		public void DisplayTopPlayersByKills(List<PlayerRankInfo> players)
		{
			Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("═══ TOP PLAYERS BY KILLS ═══\n");
			Console.ResetColor();

			DisplayPlayerTable(players, "Kills");
		}

		private void DisplayPlayerTable(List<PlayerRankInfo> players, string sortBy)
		{
			Console.WriteLine("┌──────┬────────────────────┬─────────┬──────────┬──────────┬─────────┐");
			Console.WriteLine("│ Rank │ Username           │ Score   │ W/L      │ K/D      │ WR%     │");
			Console.WriteLine("├──────┼────────────────────┼─────────┼──────────┼──────────┼─────────┤");

			foreach (var player in players.Take(10))
			{
				Console.ForegroundColor = player.Rank switch
				{
					1 => ConsoleColor.Yellow,
					2 => ConsoleColor.White,
					3 => ConsoleColor.DarkYellow,
					_ => ConsoleColor.Gray
				};

				Console.Write($"│ {player.Rank,4} │ ");
				Console.Write($"{player.Username.PadRight(18).Substring(0, 18)} │ ");
				Console.Write($"{player.Score,7} │ ");
				Console.Write($"{player.Wins,3}/{player.Losses,-3} │ ");
				Console.Write($"{player.Kills,3}/{player.Deaths,-3} │ ");
				Console.Write($"{player.WinRate,6:F1}% │");
				Console.WriteLine();
				Console.ResetColor();
			}

			Console.WriteLine("└──────┴────────────────────┴─────────┴──────────┴──────────┴─────────┘");
		}

		public void DisplayUserStats(UserStatsInfo stats)
		{
			Clear();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"═══ PLAYER STATISTICS: {stats.Username} ═══\n");
			Console.ResetColor();

			Console.WriteLine("┌───────────────────────────────────────────────────────────────┐");
			Console.WriteLine("│                     OVERALL STATISTICS                        │");
			Console.WriteLine("├───────────────────────────────────────────────────────────────┤");
			Console.WriteLine($"│ Total Games:        {stats.TotalGames,10}                              │");
			Console.WriteLine($"│ Total Wins:         {stats.TotalWins,10}                              │");
			Console.WriteLine($"│ Total Losses:       {stats.TotalLosses,10}                              │");
			Console.WriteLine($"│ Win Rate:           {stats.WinRate,9:F2}%                           │");
			Console.WriteLine("├───────────────────────────────────────────────────────────────┤");
			Console.WriteLine($"│ Total Kills:        {stats.TotalKills,10}                              │");
			Console.WriteLine($"│ Total Deaths:       {stats.TotalDeaths,10}                              │");
			Console.WriteLine($"│ K/D Ratio:          {stats.KDRatio,10:F2}                              │");
			Console.WriteLine("├───────────────────────────────────────────────────────────────┤");
			Console.WriteLine($"│ Highest Score:      {stats.HighestScore,10}                              │");
			Console.WriteLine($"│ Average Score:      {stats.AverageScore,10:F2}                              │");
			Console.WriteLine("└───────────────────────────────────────────────────────────────┘");
		}

		public void DisplayUserRank(int scoreRank, int winsRank, int killsRank)
		{
			Console.WriteLine("\n┌───────────────────────────────────────────────────────────────┐");
			Console.WriteLine("│                        RANKINGS                               │");
			Console.WriteLine("├───────────────────────────────────────────────────────────────┤");

			Console.ForegroundColor = scoreRank <= 3 ? ConsoleColor.Yellow : ConsoleColor.White;
			Console.WriteLine($"│ Score Rank:         #{scoreRank,-10}                            │");
			Console.ResetColor();

			Console.ForegroundColor = winsRank <= 3 ? ConsoleColor.Yellow : ConsoleColor.White;
			Console.WriteLine($"│ Wins Rank:          #{winsRank,-10}                            │");
			Console.ResetColor();

			Console.ForegroundColor = killsRank <= 3 ? ConsoleColor.Yellow : ConsoleColor.White;
			Console.WriteLine($"│ Kills Rank:         #{killsRank,-10}                            │");
			Console.ResetColor();

			Console.WriteLine("└───────────────────────────────────────────────────────────────┘");
		}

		public void DisplayRecentHighScores(List<HighScoreInfo> highScores)
		{
			Clear();
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("═══ RECENT HIGH SCORES ═══\n");
			Console.ResetColor();

			Console.WriteLine("┌──────────────────────┬─────────┬──────────┬─────────────────────┐");
			Console.WriteLine("│ Username             │ Score   │ K/D      │ Date                │");
			Console.WriteLine("├──────────────────────┼─────────┼──────────┼─────────────────────┤");

			foreach (var score in highScores.Take(10))
			{
				Console.Write($"│ {score.Username.PadRight(20).Substring(0, 20)} │ ");
				Console.Write($"{score.Score,7} │ ");
				Console.Write($"{score.Kills,3}/{score.Deaths,-3} │ ");
				Console.WriteLine($"{score.AchievedAt:yyyy-MM-dd HH:mm} │");
			}

			Console.WriteLine("└──────────────────────┴─────────┴──────────┴─────────────────────┘");
		}

		public void DisplayLeaderboardMenu()
		{
			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                   LEADERBOARD MENU                            ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
			Console.WriteLine("║  1. Top Players by Score                                      ║");
			Console.WriteLine("║  2. Top Players by Wins                                       ║");
			Console.WriteLine("║  3. Top Players by Kills                                      ║");
			Console.WriteLine("║  4. Recent High Scores                                        ║");
			Console.WriteLine("║  5. View My Statistics                                        ║");
			Console.WriteLine("║  6. Search Player Statistics                                  ║");
			Console.WriteLine("║  7. Back to Main Menu                                         ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
		}

		public int GetMenuChoice()
		{
			Console.Write("\nEnter your choice (1-7): ");
			if (int.TryParse(Console.ReadLine(), out int choice))
			{
				return choice;
			}
			return 0;
		}

		public string GetUsernameInput()
		{
			return GetUserInput("Enter username to search");
		}
	}
}