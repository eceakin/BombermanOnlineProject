using BombermanOnlineProject.Client.Views;
using BombermanOnlineProject.Client.Network; // SignalRClient için
using BombermanOnlineProject.Server.Core.Map;
using System.Text.Json;

namespace BombermanOnlineProject.Client.Presenters
{
	/// <summary>
	/// FASE 8.2 & 8.4 Entegrasyonu
	/// Presenter sınıfı artık doğrudan HubConnection ile değil, SignalRClient ile konuşur.
	/// </summary>
	public class GameClientPresenter
	{
		private readonly IGameView _view;
		private readonly SignalRClient _networkClient; // HubConnection yerine bu kullanılıyor

		private string _sessionId;
		private string _playerId; // Sunucudan gelen ID ile güncellenecek
		private readonly string _playerName;

		private GameMap? _currentMap;
		private Dictionary<string, (int X, int Y, bool IsAlive)> _players;
		private Dictionary<string, (int X, int Y)> _bombs;
		private Dictionary<string, List<(int X, int Y)>> _explosions;

		private bool _isGameRunning;
		private CancellationTokenSource? _gameLoopCancellation;

		public GameClientPresenter(
			IGameView view,
			string sessionId,
			string playerId,
			string playerName)
		{
			_view = view ?? throw new ArgumentNullException(nameof(view));
			_sessionId = sessionId;
			_playerId = playerId;
			_playerName = playerName;

			// SignalRClient başlatılıyor
			_networkClient = new SignalRClient();

			_players = new Dictionary<string, (int, int, bool)>();
			_bombs = new Dictionary<string, (int, int)>();
			_explosions = new Dictionary<string, List<(int, int)>>();
			_isGameRunning = false;

			SetupSignalREvents();
		}

		#region Connection & Game Lifecycle

		public async Task<bool> ConnectAndCreateGameAsync()
		{
			try
			{
				_view.DisplayMessage("Connecting to server...");
				await _networkClient.ConnectAsync();

				var resultSessionId = await _networkClient.CreateGameAsync(_playerName);

				if (string.IsNullOrEmpty(resultSessionId))
				{
					_view.DisplayError("Failed to create game session.");
					return false;
				}

				_sessionId = resultSessionId;
				_view.DisplaySuccess($"Game created: {_sessionId.Substring(0, 8)}...");
				return true;
			}
			catch (Exception ex)
			{
				_view.DisplayError($"Connection error: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> ConnectAndJoinGameAsync()
		{
			try
			{
				await _networkClient.ConnectAsync();
				var success = await _networkClient.JoinGameAsync(_sessionId, _playerName);

				if (!success)
				{
					_view.DisplayError("Failed to join game session.");
					return false;
				}

				_view.DisplaySuccess("Successfully joined game!");
				return true;
			}
			catch (Exception ex)
			{
				_view.DisplayError($"Join error: {ex.Message}");
				return false;
			}
		}

		public async Task StartGameAsync()
		{
			try
			{
				_view.Show();
				_view.ShowGameStarted();

				_isGameRunning = true;
				_gameLoopCancellation = new CancellationTokenSource();

				await _networkClient.StartGameAsync();
				await RunGameLoopAsync(_gameLoopCancellation.Token);
			}
			catch (Exception ex)
			{
				_view.DisplayError($"Game error: {ex.Message}");
				_isGameRunning = false;
			}
		}

		#endregion

		#region SignalR Event Handlers

		private void SetupSignalREvents()
		{
			// Sunucudan gelen Player ID'yi doğrula (Düzeltme: CS1061 hatası önlendi)
			_networkClient.On<JsonElement>("GameCreated", data => {
				_playerId = data.GetProperty("playerId").GetString() ?? _playerId;
			});

			_networkClient.On<JsonElement>("GameJoined", data => {
				_playerId = data.GetProperty("playerId").GetString() ?? _playerId;
			});

			_networkClient.On<JsonElement>("GameStarted", state => HandleGameStarted(state));
			_networkClient.On<JsonElement>("GameStateUpdate", state => HandleGameStateUpdate(state));
			_networkClient.On<JsonElement>("PlayerMoved", move => HandlePlayerMoved(move));
			_networkClient.On<JsonElement>("BombPlaced", bomb => HandleBombPlaced(bomb));
			_networkClient.On<JsonElement>("PlayerJoined", player => HandlePlayerJoined(player));
			_networkClient.On<JsonElement>("PlayerLeft", player => HandlePlayerLeft(player));
			_networkClient.On<string>("Error", msg => _view.DisplayError(msg));
		}

		private void HandleGameStarted(JsonElement state)
		{
			_currentMap = new GameMap();
			// Kendi oyuncumuzu listeye ekle
			_players[_playerId] = (1, 1, true);
		}

		private void HandleGameStateUpdate(JsonElement state)
		{
			try
			{
				// 1. Oyuncuları Güncelle
				var playersElement = state.GetProperty("players");
				foreach (var p in playersElement.EnumerateArray())
				{
					var id = p.GetProperty("id").GetString()!;
					var x = p.GetProperty("x").GetInt32();
					var y = p.GetProperty("y").GetInt32();
					var isAlive = p.GetProperty("isAlive").GetBoolean();

					_players[id] = (x, y, isAlive);
				}

				// 2. İstatistikleri ve Round bilgisini güncelle
				var stats = state.GetProperty("statistics");
				int playerCount = stats.GetProperty("playerCount").GetInt32();

				// View'a güncel bilgileri gönder
				_view.DisplayGameInfo(_sessionId, 1, playerCount, "Playing", TimeSpan.Zero);
			}
			catch (Exception ex)
			{
				// Loglama yapılabilir
			}
		}

		private void HandlePlayerMoved(JsonElement moveData)
		{
			try
			{
				var pid = moveData.GetProperty("playerId").GetString() ?? "";
				var x = moveData.GetProperty("x").GetInt32();
				var y = moveData.GetProperty("y").GetInt32();

				if (_players.ContainsKey(pid))
				{
					var (_, _, isAlive) = _players[pid];
					_players[pid] = (x, y, isAlive);
				}
				else
				{
					_players[pid] = (x, y, true);
				}
			}
			catch { /* Parse hatası */ }
		}

		private void HandlePlayerJoined(JsonElement playerData)
		{
			var pid = playerData.GetProperty("playerId").GetString() ?? "";
			var name = playerData.GetProperty("playerName").GetString() ?? "Unknown";
			_players[pid] = (1, 1, true);
			_view.ShowPlayerJoined(pid, name);
		}

		private void HandleBombPlaced(JsonElement bombData)
		{
			var pid = bombData.GetProperty("playerId").GetString() ?? "";
			var x = bombData.GetProperty("x").GetInt32();
			var y = bombData.GetProperty("y").GetInt32();

			var bombId = Guid.NewGuid().ToString();
			_bombs[bombId] = (x, y);
			_view.ShowBombPlaced(pid, x, y);

			// 3 saniye sonra bombayı kaldır ve patlama simülasyonu yap (Basit client-side logic)
			Task.Delay(3000).ContinueWith(_ => {
				_bombs.Remove(bombId);
				HandleExplosion(x, y);
			});
		}

		private void HandleExplosion(int x, int y)
		{
			var explosionId = Guid.NewGuid().ToString();
			var cells = new List<(int, int)> { (x, y), (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) };
			_explosions[explosionId] = cells;
			_view.ShowExplosion(x, y, cells);
			Task.Delay(500).ContinueWith(_ => _explosions.Remove(explosionId));
		}

		private void HandlePlayerLeft(JsonElement playerData)
		{
			var pid = playerData.GetProperty("playerId").GetString() ?? "";
			_players.Remove(pid);
			_view.ShowPlayerLeft(pid);
		}

		#endregion

		#region Game Loop & Input

		private async Task RunGameLoopAsync(CancellationToken token)
		{
			while (_isGameRunning && !token.IsCancellationRequested)
			{
				try
				{
					await HandleUserInputAsync();
					RenderGame();
					await Task.Delay(33, token); // ~30 FPS
				}
				catch (OperationCanceledException) { break; }
			}
			await EndGameAsync();
		}

		private async Task HandleUserInputAsync()
		{
			var key = _view.WaitForInput();
			switch (key)
			{
				case ConsoleKey.UpArrow: await _networkClient.MovePlayerAsync("up"); break;
				case ConsoleKey.DownArrow: await _networkClient.MovePlayerAsync("down"); break;
				case ConsoleKey.LeftArrow: await _networkClient.MovePlayerAsync("left"); break;
				case ConsoleKey.RightArrow: await _networkClient.MovePlayerAsync("right"); break;
				case ConsoleKey.Spacebar: await _networkClient.PlaceBombAsync(); break;
				case ConsoleKey.Escape: _isGameRunning = false; break;
			}
		}

		private void RenderGame()
		{
			if (_currentMap == null) return;

			_view.RenderMap(_currentMap, _players, _bombs, _explosions);

			if (_players.TryGetValue(_playerId, out var data))
			{
				_view.DisplayPlayerStats(_playerId, 0, 0, 0, 3.0f, 2, 1, 0);
			}

			_view.DisplayGameInfo(_sessionId, 1, _players.Count, "Playing", TimeSpan.Zero);
		}

		private async Task EndGameAsync()
		{
			_isGameRunning = false;
			await _networkClient.LeaveGameAsync();
			_view.Hide();
		}

		#endregion
	}
}