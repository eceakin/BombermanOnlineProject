using BombermanOnlineProject.Client.Views;
using BombermanOnlineProject.Server.Core.Map;
using Microsoft.AspNetCore.SignalR.Client;

namespace BombermanOnlineProject.Client.Presenters
{
	public class GameClientPresenter
	{
		private readonly IGameView _view;
		private readonly string _sessionId;
		private readonly string _playerId;
		private readonly string _playerName;
		private HubConnection? _hubConnection;
		private GameMap? _currentMap;
		private Dictionary<string, (int X, int Y, bool IsAlive)> _players;
		private Dictionary<string, (int X, int Y)> _bombs;
		private Dictionary<string, List<(int X, int Y)>> _explosions;
		private bool _isGameRunning;
		private bool _isConnected;
		private CancellationTokenSource? _gameLoopCancellation;

		public GameClientPresenter(
			IGameView view,
			string sessionId,
			string playerId,
			string playerName)
		{
			_view = view ?? throw new ArgumentNullException(nameof(view));
			_sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
			_playerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
			_playerName = playerName ?? throw new ArgumentNullException(nameof(playerName));

			_players = new Dictionary<string, (int, int, bool)>();
			_bombs = new Dictionary<string, (int, int)>();
			_explosions = new Dictionary<string, List<(int, int)>>();
			_isGameRunning = false;
			_isConnected = false;
		}

		public async Task<bool> ConnectAndCreateGameAsync()
		{
			try
			{
				await ConnectToServerAsync();

				if (!_isConnected || _hubConnection == null)
				{
					return false;
				}

				var result = await _hubConnection.InvokeAsync<string>("CreateGame", _playerName);

				if (string.IsNullOrEmpty(result))
				{
					_view.DisplayError("Failed to create game session.");
					return false;
				}

				_view.DisplaySuccess($"Game session created: {result.Substring(0, 8)}...");
				return true;
			}
			catch (Exception ex)
			{
				_view.DisplayError($"Failed to create game: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> ConnectAndJoinGameAsync()
		{
			try
			{
				await ConnectToServerAsync();

				if (!_isConnected || _hubConnection == null)
				{
					return false;
				}

				var result = await _hubConnection.InvokeAsync<bool>("JoinGame", _sessionId, _playerName);

				if (!result)
				{
					_view.DisplayError("Failed to join game session.");
					return false;
				}

				_view.DisplaySuccess("Successfully joined game!");
				return true;
			}
			catch (Exception ex)
			{
				_view.DisplayError($"Failed to join game: {ex.Message}");
				return false;
			}
		}

		public async Task StartGameAsync()
		{
			if (!_isConnected || _hubConnection == null)
			{
				_view.DisplayError("Not connected to server.");
				return;
			}

			try
			{
				_view.Show();
				_view.ShowGameStarted();

				_isGameRunning = true;
				_gameLoopCancellation = new CancellationTokenSource();

				await _hubConnection.InvokeAsync("StartGame");

				await RunGameLoopAsync(_gameLoopCancellation.Token);
			}
			catch (Exception ex)
			{
				_view.DisplayError($"Game error: {ex.Message}");
				_isGameRunning = false;
			}
		}

		private async Task ConnectToServerAsync()
		{
			try
			{
				_view.DisplayMessage("Connecting to server...");

				_hubConnection = new HubConnectionBuilder()
					.WithUrl("http://localhost:5054/gamehub")
					.WithAutomaticReconnect()
					.Build();

				SetupSignalRHandlers();

				await _hubConnection.StartAsync();

				_isConnected = true;
				_view.DisplaySuccess("Connected to server!");
				await Task.Delay(500);
			}
			catch (Exception ex)
			{
				_view.DisplayError($"Failed to connect: {ex.Message}");
				_isConnected = false;
				throw;
			}
		}

		private void SetupSignalRHandlers()
		{
			if (_hubConnection == null) return;

			_hubConnection.On<object>("GameCreated", gameData =>
			{
				_view.DisplaySuccess("Game created successfully!");
			});

			_hubConnection.On<object>("GameJoined", gameData =>
			{
				_view.DisplaySuccess("Joined game successfully!");
			});

			_hubConnection.On<object>("GameStarted", gameState =>
			{
				HandleGameStarted(gameState);
			});

			_hubConnection.On<object>("GameStateUpdate", gameState =>
			{
				HandleGameStateUpdate(gameState);
			});

			_hubConnection.On<object>("PlayerMoved", moveData =>
			{
				HandlePlayerMoved(moveData);
			});

			_hubConnection.On<object>("BombPlaced", bombData =>
			{
				HandleBombPlaced(bombData);
			});

			_hubConnection.On<object>("PlayerJoined", playerData =>
			{
				HandlePlayerJoined(playerData);
			});

			_hubConnection.On<object>("PlayerLeft", playerData =>
			{
				HandlePlayerLeft(playerData);
			});

			_hubConnection.On<string>("Error", errorMessage =>
			{
				_view.DisplayError(errorMessage);
			});

			_hubConnection.Closed += async (error) =>
			{
				_isConnected = false;
				_isGameRunning = false;
				_view.DisplayError("Connection lost. Attempting to reconnect...");
				await Task.Delay(5000);
			};
		}

		private void HandleGameStarted(object gameState)
		{
			try
			{
				_currentMap = new GameMap();

				// ÖNEMLİ: Kendi oyuncumuzu listeye eklemezsek RenderMap bizi çizmez!
				_players[_playerId] = (1, 1, true);

				_view.DisplaySuccess("Game has started!");
			}
			catch (Exception ex) { _view.DisplayError(ex.Message); }
		}

		private void HandleGameStateUpdate(object gameState)
		{
			try
			{
				var stateDict = gameState as Dictionary<string, object>;
				if (stateDict == null) return;

				if (_currentMap == null)
				{
					_currentMap = new GameMap();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating game state: {ex.Message}");
			}
		}

		private void HandlePlayerMoved(object moveData)
		{
			try
			{
				var moveDict = moveData as Dictionary<string, object>;
				if (moveDict == null) return;

				var playerId = moveDict["playerId"]?.ToString() ?? "";
				var x = Convert.ToInt32(moveDict["x"]);
				var y = Convert.ToInt32(moveDict["y"]);

				if (_players.ContainsKey(playerId))
				{
					var (_, _, isAlive) = _players[playerId];
					_players[playerId] = (x, y, isAlive);
				}
				else
				{
					_players[playerId] = (x, y, true);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error handling player move: {ex.Message}");
			}
		}

		private void HandleBombPlaced(object bombData)
		{
			try
			{
				var bombDict = bombData as Dictionary<string, object>;
				if (bombDict == null) return;

				var playerId = bombDict["playerId"]?.ToString() ?? "";
				var x = Convert.ToInt32(bombDict["x"]);
				var y = Convert.ToInt32(bombDict["y"]);

				var bombId = Guid.NewGuid().ToString();
				_bombs[bombId] = (x, y);

				_view.ShowBombPlaced(playerId, x, y);

				Task.Delay(3000).ContinueWith(_ =>
				{
					_bombs.Remove(bombId);
					HandleExplosion(x, y);
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error handling bomb placement: {ex.Message}");
			}
		}

		private void HandleExplosion(int x, int y)
		{
			try
			{
				var explosionId = Guid.NewGuid().ToString();
				var affectedCells = new List<(int, int)> { (x, y) };

				for (int i = 1; i <= 2; i++)
				{
					affectedCells.Add((x + i, y));
					affectedCells.Add((x - i, y));
					affectedCells.Add((x, y + i));
					affectedCells.Add((x, y - i));
				}

				_explosions[explosionId] = affectedCells;
				_view.ShowExplosion(x, y, affectedCells);

				Task.Delay(500).ContinueWith(_ =>
				{
					_explosions.Remove(explosionId);
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error handling explosion: {ex.Message}");
			}
		}

		private void HandlePlayerJoined(object playerData)
		{
			try
			{
				var playerDict = playerData as Dictionary<string, object>;
				if (playerDict == null) return;

				var playerId = playerDict["playerId"]?.ToString() ?? "";
				var playerName = playerDict["playerName"]?.ToString() ?? "Unknown";

				_players[playerId] = (1, 1, true); // Başlangıç koordinatı (spawn) verilmeli
				_view.ShowPlayerJoined(playerId, playerName);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error handling player join: {ex.Message}");
			}
		}

		private void HandlePlayerLeft(object playerData)
		{
			try
			{
				var playerDict = playerData as Dictionary<string, object>;
				if (playerDict == null) return;

				var playerId = playerDict["playerId"]?.ToString() ?? "";

				_players.Remove(playerId);
				_view.ShowPlayerLeft(playerId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error handling player leave: {ex.Message}");
			}
		}

		private async Task RunGameLoopAsync(CancellationToken cancellationToken)
		{
			if (_currentMap == null)
			{
				_currentMap = new GameMap();
			}

			while (_isGameRunning && !cancellationToken.IsCancellationRequested)
			{
				try
				{
					await HandleUserInputAsync();

					RenderGame();

					await Task.Delay(33, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					_view.DisplayError($"Game loop error: {ex.Message}");
				}
			}

			await EndGameAsync();
		}

		private async Task HandleUserInputAsync()
		{
			if (_hubConnection == null || !_isConnected) return;

			var key = _view.WaitForInput();

			try
			{
				switch (key)
				{
					case ConsoleKey.UpArrow:
						await _hubConnection.InvokeAsync("MovePlayer", "up");
						break;
					case ConsoleKey.DownArrow:
						await _hubConnection.InvokeAsync("MovePlayer", "down");
						break;
					case ConsoleKey.LeftArrow:
						await _hubConnection.InvokeAsync("MovePlayer", "left");
						break;
					case ConsoleKey.RightArrow:
						await _hubConnection.InvokeAsync("MovePlayer", "right");
						break;
					case ConsoleKey.Spacebar:
						await _hubConnection.InvokeAsync("PlaceBomb");
						break;
					case ConsoleKey.P:
						_isGameRunning = false;
						_view.DisplayMessage("Game paused. Press any key to return to menu...");
						break;
					case ConsoleKey.Escape:
						_isGameRunning = false;
						break;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error sending input: {ex.Message}");
			}
		}

		private void RenderGame()
		{
			if (_currentMap == null) return;

			_view.RenderMap(_currentMap, _players, _bombs, _explosions);

			if (_players.TryGetValue(_playerId, out var playerData))
			{
				_view.DisplayPlayerStats(
					_playerId,
					score: 0,
					kills: 0,
					deaths: 0,
					speed: 3.0f,
					bombPower: 2,
					maxBombs: 1,
					activeBombs: 0);
			}

			_view.DisplayGameInfo(
				_sessionId,
				currentRound: 1,
				playerCount: _players.Count,
				gameState: "Playing",
				duration: TimeSpan.Zero);
		}

		private async Task EndGameAsync()
		{
			_isGameRunning = false;

			var finalScores = new Dictionary<string, int>
			{
				{ _playerId, 0 }
			};

			_view.ShowGameEnded("Winner", finalScores);

			if (_hubConnection != null && _isConnected)
			{
				try
				{
					await _hubConnection.InvokeAsync("LeaveGame");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error leaving game: {ex.Message}");
				}
			}

			_view.Hide();
		}

		public async Task DisconnectAsync()
		{
			_isGameRunning = false;
			_gameLoopCancellation?.Cancel();

			if (_hubConnection != null)
			{
				try
				{
					if (_isConnected)
					{
						await _hubConnection.InvokeAsync("LeaveGame");
					}

					await _hubConnection.StopAsync();
					await _hubConnection.DisposeAsync();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error disconnecting: {ex.Message}");
				}
				finally
				{
					_hubConnection = null;
					_isConnected = false;
				}
			}
		}
	}
}