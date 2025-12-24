using BombermanOnlineProject.Client.Views;

namespace BombermanOnlineProject.Client.Presenters
{
	public class MenuPresenter
	{
		private readonly IView _mainView;
		private readonly ILobbyView _lobbyView;
		private readonly ILeaderboardView _leaderboardView;
		private readonly IGameView _gameView;
		private GameClientPresenter? _gamePresenter;
		private string? _currentUserId;
		private string? _currentUsername;

		public MenuPresenter(
			IView mainView,
			ILobbyView lobbyView,
			ILeaderboardView leaderboardView,
			IGameView gameView)
		{
			_mainView = mainView ?? throw new ArgumentNullException(nameof(mainView));
			_lobbyView = lobbyView ?? throw new ArgumentNullException(nameof(lobbyView));
			_leaderboardView = leaderboardView ?? throw new ArgumentNullException(nameof(leaderboardView));
			_gameView = gameView ?? throw new ArgumentNullException(nameof(gameView));
		}

		public async Task RunAsync()
		{
			_mainView.Show();
			DisplayWelcomeBanner();

			await AuthenticateUserAsync();

			bool running = true;
			while (running)
			{
				try
				{
					DisplayMainMenu();
					var choice = GetMenuChoice();

					switch (choice)
					{
						case 1:
							await ShowLobbyMenuAsync();
							break;
						case 2:
							await ShowLeaderboardMenuAsync();
							break;
						case 3:
							await ShowUserProfileAsync();
							break;
						case 4:
							await ShowSettingsMenuAsync();
							break;
						case 5:
							running = await ConfirmExitAsync();
							break;
						default:
							_mainView.DisplayError("Invalid choice. Please try again.");
							await Task.Delay(1000);
							break;
					}
				}
				catch (Exception ex)
				{
					_mainView.DisplayError($"An error occurred: {ex.Message}");
					await Task.Delay(2000);
				}
			}

			_mainView.DisplaySuccess("Thank you for playing Bomberman Online!");
			await Task.Delay(1000);
			_mainView.Hide();
		}

		private void DisplayWelcomeBanner()
		{
			_mainView.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════╗
║                                                               ║
║    ██████╗  ██████╗ ███╗   ███╗██████╗ ███████╗██████╗       ║
║    ██╔══██╗██╔═══██╗████╗ ████║██╔══██╗██╔════╝██╔══██╗      ║
║    ██████╔╝██║   ██║██╔████╔██║██████╔╝█████╗  ██████╔╝      ║
║    ██╔══██╗██║   ██║██║╚██╔╝██║██╔══██╗██╔══╝  ██╔══██╗      ║
║    ██████╔╝╚██████╔╝██║ ╚═╝ ██║██████╔╝███████╗██║  ██║      ║
║    ╚═════╝  ╚═════╝ ╚═╝     ╚═╝╚═════╝ ╚══════╝╚═╝  ╚═╝      ║
║                                                               ║
║                   ONLINE MULTIPLAYER                          ║
║                                                               ║
╚═══════════════════════════════════════════════════════════════╝
");
			Console.ResetColor();
		}

		private async Task AuthenticateUserAsync()
		{
			_mainView.Clear();
			DisplayWelcomeBanner();

			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                      AUTHENTICATION                           ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

			_currentUserId = Guid.NewGuid().ToString();
			_currentUsername = _mainView.GetUserInput("Enter your username");

			if (string.IsNullOrWhiteSpace(_currentUsername))
			{
				_currentUsername = $"Player_{new Random().Next(1000, 9999)}";
			}

			_mainView.DisplaySuccess($"Welcome, {_currentUsername}!");
			await Task.Delay(1500);
		}

		private void DisplayMainMenu()
		{
			_mainView.Clear();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"\nWelcome, {_currentUsername}!");
			Console.ResetColor();

			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                        MAIN MENU                              ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("║  1. Play Game (Lobby)                                         ║");
			Console.WriteLine("║  2. Leaderboard                                               ║");
			Console.WriteLine("║  3. My Profile                                                ║");
			Console.WriteLine("║  4. Settings                                                  ║");
			Console.WriteLine("║  5. Exit                                                      ║");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
		}

		private int GetMenuChoice()
		{
			Console.Write("\nEnter your choice (1-5): ");
			if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= 5)
			{
				return choice;
			}
			return 0;
		}

		private async Task ShowLobbyMenuAsync()
		{
			_lobbyView.Show();

			bool inLobby = true;
			while (inLobby)
			{
				_lobbyView.DisplayLobbyMenu();
				var choice = _lobbyView.GetMenuChoice();

				switch (choice)
				{
					case 1:
						await CreateNewLobbyAsync();
						break;
					case 2:
						await JoinExistingLobbyAsync();
						break;
					case 3:
						await ViewAvailableLobbiesAsync();
						break;
					case 4:
						inLobby = false;
						break;
					default:
						_lobbyView.DisplayError("Invalid choice.");
						await Task.Delay(1000);
						break;
				}
			}

			_lobbyView.Hide();
		}

		private async Task CreateNewLobbyAsync()
		{
			try
			{
				var playerName = _lobbyView.GetPlayerNameInput();
				if (string.IsNullOrWhiteSpace(playerName))
				{
					playerName = _currentUsername ?? "Player";
				}

				var theme = _lobbyView.GetThemeChoice();

				_lobbyView.DisplayMessage($"Creating lobby with theme: {theme}...");

				var sessionId = Guid.NewGuid().ToString();

				_lobbyView.ShowLobbyCreated(sessionId);

				_gamePresenter = new GameClientPresenter(
					_gameView,
					sessionId,
					_currentUserId!,
					playerName);

				await _gamePresenter.ConnectAndCreateGameAsync();

				_lobbyView.DisplayMessage("\nWaiting for opponent to join...");
				_lobbyView.DisplayMessage("Press 'S' to start game (when ready) or 'Q' to cancel");

				bool waiting = true;
				while (waiting)
				{
					if (Console.KeyAvailable)
					{
						var key = Console.ReadKey(true).Key;
						if (key == ConsoleKey.S)
						{
							await StartGameAsync();
							waiting = false;
						}
						else if (key == ConsoleKey.Q)
						{
							await _gamePresenter.DisconnectAsync();
							_gamePresenter = null;
							waiting = false;
						}
					}
					await Task.Delay(100);
				}
			}
			catch (Exception ex)
			{
				_lobbyView.DisplayError($"Failed to create lobby: {ex.Message}");
				await Task.Delay(2000);
			}
		}

		private async Task JoinExistingLobbyAsync()
		{
			try
			{
				var sessionId = _lobbyView.GetSessionIdInput();
				if (string.IsNullOrWhiteSpace(sessionId))
				{
					_lobbyView.DisplayError("Invalid session ID.");
					await Task.Delay(1500);
					return;
				}

				var playerName = _lobbyView.GetPlayerNameInput();
				if (string.IsNullOrWhiteSpace(playerName))
				{
					playerName = _currentUsername ?? "Player";
				}

				_lobbyView.DisplayMessage($"Joining lobby {sessionId.Substring(0, 8)}...");

				_gamePresenter = new GameClientPresenter(
					_gameView,
					sessionId,
					_currentUserId!,
					playerName);

				var joined = await _gamePresenter.ConnectAndJoinGameAsync();

				if (joined)
				{
					_lobbyView.DisplaySuccess("Successfully joined the lobby!");
					await Task.Delay(1500);

					await StartGameAsync();
				}
				else
				{
					_lobbyView.DisplayError("Failed to join lobby. It may be full or not exist.");
					_gamePresenter = null;
					await Task.Delay(2000);
				}
			}
			catch (Exception ex)
			{
				_lobbyView.DisplayError($"Failed to join lobby: {ex.Message}");
				_gamePresenter = null;
				await Task.Delay(2000);
			}
		}

		private async Task ViewAvailableLobbiesAsync()
		{
			_lobbyView.DisplayMessage("Fetching available lobbies...");

			await Task.Delay(500);

			var dummyLobbies = new List<LobbyInfo>
			{
				new LobbyInfo
				{
					SessionId = Guid.NewGuid().ToString(),
					HostPlayerName = "Player123",
					CurrentPlayers = 1,
					MaxPlayers = 2,
					Theme = "Forest",
					Status = "Waiting"
				}
			};

			_lobbyView.DisplayLobbyList(dummyLobbies);
			_lobbyView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
		}

		private async Task StartGameAsync()
		{
			if (_gamePresenter == null)
			{
				_lobbyView.DisplayError("No active game session.");
				await Task.Delay(1500);
				return;
			}

			_lobbyView.ShowLobbyStarting();
			_lobbyView.Hide();

			await _gamePresenter.StartGameAsync();

			_gamePresenter = null;
		}

		private async Task ShowLeaderboardMenuAsync()
		{
			_leaderboardView.Show();

			bool inLeaderboard = true;
			while (inLeaderboard)
			{
				_leaderboardView.DisplayLeaderboardMenu();
				var choice = _leaderboardView.GetMenuChoice();

				switch (choice)
				{
					case 1:
						await ShowTopPlayersByScoreAsync();
						break;
					case 2:
						await ShowTopPlayersByWinsAsync();
						break;
					case 3:
						await ShowTopPlayersByKillsAsync();
						break;
					case 4:
						await ShowRecentHighScoresAsync();
						break;
					case 5:
						await ShowMyStatisticsAsync();
						break;
					case 6:
						await SearchPlayerStatisticsAsync();
						break;
					case 7:
						inLeaderboard = false;
						break;
					default:
						_leaderboardView.DisplayError("Invalid choice.");
						await Task.Delay(1000);
						break;
				}
			}

			_leaderboardView.Hide();
		}

		private async Task ShowTopPlayersByScoreAsync()
		{
			_leaderboardView.DisplayMessage("Loading top players by score...");
			await Task.Delay(500);

			var dummyPlayers = GenerateDummyPlayerRankings();
			_leaderboardView.DisplayTopPlayersByScore(dummyPlayers);

			_leaderboardView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
		}

		private async Task ShowTopPlayersByWinsAsync()
		{
			_leaderboardView.DisplayMessage("Loading top players by wins...");
			await Task.Delay(500);

			var dummyPlayers = GenerateDummyPlayerRankings();
			_leaderboardView.DisplayTopPlayersByWins(dummyPlayers);

			_leaderboardView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
		}

		private async Task ShowTopPlayersByKillsAsync()
		{
			_leaderboardView.DisplayMessage("Loading top players by kills...");
			await Task.Delay(500);

			var dummyPlayers = GenerateDummyPlayerRankings();
			_leaderboardView.DisplayTopPlayersByKills(dummyPlayers);

			_leaderboardView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
		}

		private async Task ShowRecentHighScoresAsync()
		{
			_leaderboardView.DisplayMessage("Loading recent high scores...");
			await Task.Delay(500);

			var dummyHighScores = new List<HighScoreInfo>
			{
				new HighScoreInfo
				{
					Username = "ProGamer123",
					Score = 5000,
					Kills = 15,
					Deaths = 3,
					AchievedAt = DateTime.Now.AddHours(-2),
					MapTheme = "Forest"
				},
				new HighScoreInfo
				{
					Username = "BombMaster",
					Score = 4500,
					Kills = 12,
					Deaths = 5,
					AchievedAt = DateTime.Now.AddHours(-5),
					MapTheme = "City"
				}
			};

			_leaderboardView.DisplayRecentHighScores(dummyHighScores);

			_leaderboardView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
		}

		private async Task ShowMyStatisticsAsync()
		{
			_leaderboardView.DisplayMessage("Loading your statistics...");
			await Task.Delay(500);

			var dummyStats = new UserStatsInfo
			{
				Username = _currentUsername ?? "Player",
				TotalGames = 25,
				TotalWins = 15,
				TotalLosses = 10,
				TotalKills = 87,
				TotalDeaths = 45,
				HighestScore = 3500,
				WinRate = 60.0,
				KDRatio = 1.93,
				AverageScore = 1850.5
			};

			_leaderboardView.DisplayUserStats(dummyStats);
			_leaderboardView.DisplayUserRank(5, 8, 12);

			_leaderboardView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
		}

		private async Task SearchPlayerStatisticsAsync()
		{
			var username = _leaderboardView.GetUsernameInput();
			if (string.IsNullOrWhiteSpace(username))
			{
				_leaderboardView.DisplayError("Invalid username.");
				await Task.Delay(1500);
				return;
			}

			_leaderboardView.DisplayMessage($"Searching for player: {username}...");
			await Task.Delay(500);

			var dummyStats = new UserStatsInfo
			{
				Username = username,
				TotalGames = 50,
				TotalWins = 30,
				TotalLosses = 20,
				TotalKills = 150,
				TotalDeaths = 80,
				HighestScore = 4200,
				WinRate = 60.0,
				KDRatio = 1.88,
				AverageScore = 2100.0
			};

			_leaderboardView.DisplayUserStats(dummyStats);
			_leaderboardView.DisplayUserRank(3, 5, 7);

			_leaderboardView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
		}

		private async Task ShowUserProfileAsync()
		{
			_mainView.Clear();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                        USER PROFILE                           ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
			Console.ResetColor();

			Console.WriteLine($"\nUsername: {_currentUsername}");
			Console.WriteLine($"User ID: {_currentUserId?.Substring(0, 8)}...");
			Console.WriteLine($"Account Created: {DateTime.Now.AddDays(-30):yyyy-MM-dd}");

			Console.WriteLine("\nProfile management features coming soon!");

			_mainView.DisplayMessage("\nPress any key to continue...");
			Console.ReadKey();
			await Task.CompletedTask;
		}

		private async Task ShowSettingsMenuAsync()
		{
			_mainView.Clear();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                          SETTINGS                             ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
			Console.ResetColor();

			Console.WriteLine("\n1. Sound Settings");
			Console.WriteLine("2. Display Settings");
			Console.WriteLine("3. Control Settings");
			Console.WriteLine("4. Back to Main Menu");

			Console.Write("\nEnter your choice (1-4): ");
			var choice = Console.ReadLine();

			_mainView.DisplayMessage("\nSettings features coming soon!");
			await Task.Delay(1500);
		}

		private async Task<bool> ConfirmExitAsync()
		{
			var confirm = _mainView.ConfirmAction("\nAre you sure you want to exit?");
			if (confirm && _gamePresenter != null)
			{
				await _gamePresenter.DisconnectAsync();
			}
			return confirm;
		}

		private List<PlayerRankInfo> GenerateDummyPlayerRankings()
		{
			return new List<PlayerRankInfo>
			{
				new PlayerRankInfo { Rank = 1, Username = "ProGamer123", Score = 5000, Wins = 45, Losses = 10, Kills = 200, Deaths = 50, WinRate = 81.8, KDRatio = 4.0 },
				new PlayerRankInfo { Rank = 2, Username = "BombMaster", Score = 4500, Wins = 40, Losses = 15, Kills = 180, Deaths = 60, WinRate = 72.7, KDRatio = 3.0 },
				new PlayerRankInfo { Rank = 3, Username = "ExplosionKing", Score = 4200, Wins = 38, Losses = 17, Kills = 170, Deaths = 65, WinRate = 69.1, KDRatio = 2.6 },
				new PlayerRankInfo { Rank = 4, Username = "TNT_Expert", Score = 4000, Wins = 35, Losses = 20, Kills = 160, Deaths = 70, WinRate = 63.6, KDRatio = 2.3 },
				new PlayerRankInfo { Rank = 5, Username = _currentUsername ?? "You", Score = 3500, Wins = 30, Losses = 25, Kills = 140, Deaths = 80, WinRate = 54.5, KDRatio = 1.8 },
				new PlayerRankInfo { Rank = 6, Username = "BlastZone", Score = 3200, Wins = 28, Losses = 27, Kills = 130, Deaths = 85, WinRate = 50.9, KDRatio = 1.5 },
				new PlayerRankInfo { Rank = 7, Username = "BomberJack", Score = 3000, Wins = 25, Losses = 30, Kills = 120, Deaths = 90, WinRate = 45.5, KDRatio = 1.3 },
				new PlayerRankInfo { Rank = 8, Username = "Dynamite", Score = 2800, Wins = 22, Losses = 33, Kills = 110, Deaths = 95, WinRate = 40.0, KDRatio = 1.2 },
				new PlayerRankInfo { Rank = 9, Username = "KaboomKid", Score = 2500, Wins = 20, Losses = 35, Kills = 100, Deaths = 100, WinRate = 36.4, KDRatio = 1.0 },
				new PlayerRankInfo { Rank = 10, Username = "BombSquad", Score = 2200, Wins = 18, Losses = 37, Kills = 90, Deaths = 105, WinRate = 32.7, KDRatio = 0.9 }
			};
		}
	}
}