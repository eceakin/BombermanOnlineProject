using BombermanOnlineProject.Server.Core.Game;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BombermanOnlineProject.Server.Hubs
{
	public class GameHub : Hub
	{
		private static readonly ConcurrentDictionary<string, string> _connectionToPlayer = new();
		private static readonly ConcurrentDictionary<string, Timer> _gameLoopTimers = new();
		private const int GAME_TICK_RATE = 33;

		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();
			Console.WriteLine($"[GameHub] Client connected: {Context.ConnectionId}");
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			if (_connectionToPlayer.TryRemove(Context.ConnectionId, out var playerId))
			{
				await LeaveGame();
				Console.WriteLine($"[GameHub] Player {playerId} disconnected");
			}

			await base.OnDisconnectedAsync(exception);
		}

		public async Task<string> CreateGame(string playerName)
		{
			try
			{
				var playerId = Guid.NewGuid().ToString();
				_connectionToPlayer.TryAdd(Context.ConnectionId, playerId);

				var session = GameManager.Instance.CreateGameSession(playerId);
				session.AddPlayer(playerId);

				await Groups.AddToGroupAsync(Context.ConnectionId, session.SessionId);

				await Clients.Caller.SendAsync("GameCreated", new
				{
					sessionId = session.SessionId,
					playerId = playerId,
					playerName = playerName
				});

				Console.WriteLine($"[GameHub] Game created: {session.SessionId} by {playerId}");
				return session.SessionId;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("Error", ex.Message);
				return string.Empty;
			}
		}

		public async Task<bool> JoinGame(string sessionId, string playerName)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					await Clients.Caller.SendAsync("Error", "Game session not found");
					return false;
				}

				if (session.State != GameState.Waiting)
				{
					await Clients.Caller.SendAsync("Error", "Game already started");
					return false;
				}

				var playerId = Guid.NewGuid().ToString();
				_connectionToPlayer.TryAdd(Context.ConnectionId, playerId);

				if (!GameManager.Instance.JoinGameSession(sessionId, playerId))
				{
					await Clients.Caller.SendAsync("Error", "Failed to join game");
					return false;
				}

				await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

				await Clients.Caller.SendAsync("GameJoined", new
				{
					sessionId = sessionId,
					playerId = playerId,
					playerName = playerName
				});

				await Clients.Group(sessionId).SendAsync("PlayerJoined", new
				{
					playerId = playerId,
					playerName = playerName
				});

				Console.WriteLine($"[GameHub] Player {playerId} joined game {sessionId}");
				return true;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("Error", ex.Message);
				return false;
			}
		}

		public async Task StartGame()
		{
			try
			{
				if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var playerId))
				{
					await Clients.Caller.SendAsync("Error", "Player not found");
					return;
				}

				var sessionId = GameManager.Instance.GetPlayerSession(playerId);
				if (sessionId == null)
				{
					await Clients.Caller.SendAsync("Error", "Session not found");
					return;
				}

				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null || session.HostPlayerId != playerId)
				{
					await Clients.Caller.SendAsync("Error", "Only host can start the game");
					return;
				}

				session.StartGame();

				var gameState = BuildGameState(session);
				await Clients.Group(sessionId).SendAsync("GameStarted", gameState);

				StartGameLoop(sessionId);

				Console.WriteLine($"[GameHub] Game started: {sessionId}");
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("Error", ex.Message);
			}
		}

		public async Task MovePlayer(string direction)
		{
			try
			{
				if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var playerId))
				{
					return;
				}

				var sessionId = GameManager.Instance.GetPlayerSession(playerId);
				if (sessionId == null)
				{
					return;
				}

				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null || session.State != GameState.Playing)
				{
					return;
				}

				var player = session.GetPlayer(playerId);
				if (player == null || !player.IsAlive)
				{
					return;
				}

				int newX = player.X;
				int newY = player.Y;

				switch (direction.ToLower())
				{
					case "up":
						newY--;
						break;
					case "down":
						newY++;
						break;
					case "left":
						newX--;
						break;
					case "right":
						newX++;
						break;
					default:
						return;
				}

				if (session.MovePlayer(playerId, newX, newY))
				{
					await Clients.Group(sessionId).SendAsync("PlayerMoved", new
					{
						playerId = playerId,
						x = newX,
						y = newY
					});
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GameHub] MovePlayer error: {ex.Message}");
			}
		}

		public async Task PlaceBomb()
		{
			try
			{
				if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var playerId))
				{
					return;
				}

				var sessionId = GameManager.Instance.GetPlayerSession(playerId);
				if (sessionId == null)
				{
					return;
				}

				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null || session.State != GameState.Playing)
				{
					return;
				}

				if (session.PlaceBomb(playerId))
				{
					var player = session.GetPlayer(playerId);
					if (player != null)
					{
						await Clients.Group(sessionId).SendAsync("BombPlaced", new
						{
							playerId = playerId,
							x = player.X,
							y = player.Y,
							power = player.BombPower
						});
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GameHub] PlaceBomb error: {ex.Message}");
			}
		}

		public async Task LeaveGame()
		{
			try
			{
				if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var playerId))
				{
					return;
				}

				var sessionId = GameManager.Instance.GetPlayerSession(playerId);
				if (sessionId == null)
				{
					return;
				}

				GameManager.Instance.LeaveGameSession(playerId);
				_connectionToPlayer.TryRemove(Context.ConnectionId, out _);

				await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);

				await Clients.Group(sessionId).SendAsync("PlayerLeft", new
				{
					playerId = playerId
				});

				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null || session.IsEmpty())
				{
					StopGameLoop(sessionId);
					GameManager.Instance.RemoveGameSession(sessionId);
				}

				Console.WriteLine($"[GameHub] Player {playerId} left game {sessionId}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GameHub] LeaveGame error: {ex.Message}");
			}
		}

		public async Task GetGameState()
		{
			try
			{
				if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var playerId))
				{
					await Clients.Caller.SendAsync("Error", "Player not found");
					return;
				}

				var sessionId = GameManager.Instance.GetPlayerSession(playerId);
				if (sessionId == null)
				{
					await Clients.Caller.SendAsync("Error", "Session not found");
					return;
				}

				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					await Clients.Caller.SendAsync("Error", "Session not found");
					return;
				}

				var gameState = BuildGameState(session);
				await Clients.Caller.SendAsync("GameState", gameState);
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("Error", ex.Message);
			}
		}

		private void StartGameLoop(string sessionId)
		{
			if (_gameLoopTimers.ContainsKey(sessionId))
			{
				return;
			}

			var timer = new Timer(async _ =>
			{
				try
				{
					var session = GameManager.Instance.GetGameSession(sessionId);
					if (session == null || session.State != GameState.Playing)
					{
						StopGameLoop(sessionId);
						return;
					}

					var gameState = BuildGameState(session);
					await Clients.Group(sessionId).SendAsync("GameStateUpdate", gameState);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[GameHub] GameLoop error: {ex.Message}");
				}
			}, null, 0, GAME_TICK_RATE);

			_gameLoopTimers.TryAdd(sessionId, timer);
		}

		private void StopGameLoop(string sessionId)
		{
			if (_gameLoopTimers.TryRemove(sessionId, out var timer))
			{
				timer.Dispose();
			}
		}

		private object BuildGameState(GameSession session)
		{
			var players = session.GetPlayerIds()
				.Select(id => session.GetPlayer(id))
				.Where(p => p != null)
				.Select(p => new
				{
					id = p!.Id,
					name = p.PlayerName,
					x = p.X,
					y = p.Y,
					isAlive = p.IsAlive,
					score = p.Score,
					speed = p.Speed,
					bombPower = p.BombPower,
					maxBombs = p.MaxBombs,
					activeBombs = p.ActiveBombs,
					kills = p.Kills,
					deaths = p.Deaths,
					isInvulnerable = p.IsInvulnerable
				}).ToList();

			var stats = session.GetGameStatistics();

			return new
			{
				sessionId = session.SessionId,
				state = session.State.ToString(),
				currentRound = session.CurrentRound,
				players = players,
				mapWidth = session.Map.Width,
				mapHeight = session.Map.Height,
				statistics = new
				{
					playerCount = stats.PlayerCount,
					currentRound = stats.CurrentRound,
					activeBombs = stats.ActiveBombs,
					activeExplosions = stats.ActiveExplosions,
					activeEnemies = stats.ActiveEnemies,
					activePowerUps = stats.ActivePowerUps,
					duration = stats.Duration.TotalSeconds
				}
			};
		}
	}
}