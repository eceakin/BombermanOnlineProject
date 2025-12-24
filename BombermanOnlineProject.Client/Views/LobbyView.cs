namespace BombermanOnlineProject.Client.Views
{
	public class LobbyView : ILobbyView
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
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("║              BOMBERMAN ONLINE - LOBBY                         ║");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
			Console.ResetColor();
			Console.WriteLine();
		}

		public void DisplayLobbyList(List<LobbyInfo> lobbies)
		{
			Clear();

			if (lobbies == null || !lobbies.Any())
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("No lobbies available. Create a new one!");
				Console.ResetColor();
				return;
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Available Lobbies:");
			Console.WriteLine("─────────────────────────────────────────────────────────────");
			Console.ResetColor();

			int index = 1;
			foreach (var lobby in lobbies)
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"{index}. ");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Write($"[{lobby.SessionId.Substring(0, 8)}...] ");
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write($"Host: {lobby.HostPlayerName} | ");
				Console.Write($"Players: {lobby.CurrentPlayers}/{lobby.MaxPlayers} | ");
				Console.Write($"Theme: {lobby.Theme} | ");
				Console.ForegroundColor = lobby.Status == "Waiting" ? ConsoleColor.Green : ConsoleColor.Red;
				Console.WriteLine($"Status: {lobby.Status}");
				Console.ResetColor();
				index++;
			}

			Console.WriteLine("─────────────────────────────────────────────────────────────");
		}

		public void DisplayLobbyDetails(LobbyInfo lobby)
		{
			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                    LOBBY DETAILS                              ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
			Console.WriteLine($"Session ID: {lobby.SessionId}");
			Console.WriteLine($"Host: {lobby.HostPlayerName}");
			Console.WriteLine($"Players: {lobby.CurrentPlayers}/{lobby.MaxPlayers}");
			Console.WriteLine($"Theme: {lobby.Theme}");
			Console.WriteLine($"Status: {lobby.Status}");
			Console.WriteLine($"Created: {lobby.CreatedAt:yyyy-MM-dd HH:mm:ss}");
			Console.WriteLine("─────────────────────────────────────────────────────────────");
		}

		public void ShowLobbyCreated(string sessionId)
		{
			DisplaySuccess($"Lobby created successfully!");
			Console.WriteLine($"Session ID: {sessionId}");
			Console.WriteLine("Waiting for players to join...");
		}

		public void ShowPlayerJoinedLobby(string playerName)
		{
			DisplaySuccess($"Player '{playerName}' joined the lobby");
		}

		public void ShowPlayerLeftLobby(string playerName)
		{
			DisplayMessage($"Player '{playerName}' left the lobby");
		}

		public void ShowLobbyFull()
		{
			DisplayError("Lobby is full! Cannot join.");
		}

		public void ShowLobbyNotFound()
		{
			DisplayError("Lobby not found! Please check the session ID.");
		}

		public void ShowWaitingForPlayers(int currentPlayers, int maxPlayers)
		{
			Console.WriteLine($"\nWaiting for players... ({currentPlayers}/{maxPlayers})");
			Console.WriteLine("Press 'S' to start the game or 'Q' to quit");
		}

		public void ShowLobbyStarting()
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("║                    STARTING GAME...                           ║");
			Console.WriteLine("║                                                               ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
			Console.ResetColor();
			Thread.Sleep(2000);
		}

		public void DisplayLobbyMenu()
		{
			Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
			Console.WriteLine("║                     LOBBY MENU                                ║");
			Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
			Console.WriteLine("║  1. Create New Lobby                                          ║");
			Console.WriteLine("║  2. Join Existing Lobby                                       ║");
			Console.WriteLine("║  3. View Available Lobbies                                    ║");
			Console.WriteLine("║  4. Back to Main Menu                                         ║");
			Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
		}

		public int GetMenuChoice()
		{
			Console.Write("\nEnter your choice (1-4): ");
			if (int.TryParse(Console.ReadLine(), out int choice))
			{
				return choice;
			}
			return 0;
		}

		public string GetSessionIdInput()
		{
			return GetUserInput("Enter Session ID");
		}

		public string GetPlayerNameInput()
		{
			return GetUserInput("Enter your player name");
		}

		public string GetThemeChoice()
		{
			Console.WriteLine("\nChoose a theme:");
			Console.WriteLine("1. Forest (Default)");
			Console.WriteLine("2. Desert");
			Console.WriteLine("3. City");
			Console.Write("\nEnter your choice (1-3): ");

			var choice = Console.ReadLine();
			return choice switch
			{
				"2" => "Desert",
				"3" => "City",
				_ => "Forest"
			};
		}
	}
}