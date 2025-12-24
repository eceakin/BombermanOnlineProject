using BombermanOnlineProject.Server.Core.Game;
using System.Collections.Concurrent;

namespace BombermanOnlineProject.Server.Presenters
{
	public class LobbyPresenter
	{
		private readonly ConcurrentDictionary<string, LobbyInfo> _lobbies;
		private readonly object _lobbyLock;

		public LobbyPresenter()
		{
			_lobbies = new ConcurrentDictionary<string, LobbyInfo>();
			_lobbyLock = new object();
		}

		public LobbyInfo CreateLobby(string sessionId, string hostPlayerId, string hostPlayerName, string theme = "Forest")
		{
			var lobbyInfo = new LobbyInfo
			{
				SessionId = sessionId,
				HostPlayerId = hostPlayerId,
				HostPlayerName = hostPlayerName,
				Theme = theme,
				CreatedAt = DateTime.UtcNow,
				Status = LobbyStatus.Waiting,
				MaxPlayers = 2,
				CurrentPlayers = 1
			};

			_lobbies.TryAdd(sessionId, lobbyInfo);
			Console.WriteLine($"[LobbyPresenter] Lobby created: {sessionId}");
			return lobbyInfo;
		}

		public bool AddPlayerToLobby(string sessionId, string playerId, string playerName)
		{
			if (_lobbies.TryGetValue(sessionId, out var lobby))
			{
				lock (_lobbyLock)
				{
					if (lobby.CurrentPlayers >= lobby.MaxPlayers)
					{
						return false;
					}

					if (lobby.Status != LobbyStatus.Waiting)
					{
						return false;
					}

					lobby.PlayerIds.Add(playerId);
					lobby.PlayerNames.Add(playerName);
					lobby.CurrentPlayers++;

					Console.WriteLine($"[LobbyPresenter] Player {playerId} joined lobby {sessionId}");
					return true;
				}
			}

			return false;
		}

		public bool RemovePlayerFromLobby(string sessionId, string playerId)
		{
			if (_lobbies.TryGetValue(sessionId, out var lobby))
			{
				lock (_lobbyLock)
				{
					var index = lobby.PlayerIds.IndexOf(playerId);
					if (index >= 0)
					{
						lobby.PlayerIds.RemoveAt(index);
						lobby.PlayerNames.RemoveAt(index);
						lobby.CurrentPlayers--;

						if (lobby.CurrentPlayers == 0 || playerId == lobby.HostPlayerId)
						{
							_lobbies.TryRemove(sessionId, out _);
							Console.WriteLine($"[LobbyPresenter] Lobby {sessionId} removed");
							return true;
						}

						Console.WriteLine($"[LobbyPresenter] Player {playerId} left lobby {sessionId}");
						return true;
					}
				}
			}

			return false;
		}

		public LobbyInfo? GetLobby(string sessionId)
		{
			_lobbies.TryGetValue(sessionId, out var lobby);
			return lobby;
		}

		public List<LobbyInfo> GetAllLobbies()
		{
			return _lobbies.Values.ToList();
		}

		public List<LobbyInfo> GetAvailableLobbies()
		{
			return _lobbies.Values
				.Where(l => l.Status == LobbyStatus.Waiting && l.CurrentPlayers < l.MaxPlayers)
				.OrderBy(l => l.CreatedAt)
				.ToList();
		}

		public bool StartLobby(string sessionId)
		{
			if (_lobbies.TryGetValue(sessionId, out var lobby))
			{
				lock (_lobbyLock)
				{
					if (lobby.Status == LobbyStatus.Waiting && lobby.CurrentPlayers >= 1)
					{
						lobby.Status = LobbyStatus.InProgress;
						lobby.StartedAt = DateTime.UtcNow;
						Console.WriteLine($"[LobbyPresenter] Lobby {sessionId} started");
						return true;
					}
				}
			}

			return false;
		}

		public bool FinishLobby(string sessionId)
		{
			if (_lobbies.TryRemove(sessionId, out var lobby))
			{
				Console.WriteLine($"[LobbyPresenter] Lobby {sessionId} finished");
				return true;
			}

			return false;
		}

		public bool UpdateLobbyTheme(string sessionId, string theme)
		{
			if (_lobbies.TryGetValue(sessionId, out var lobby))
			{
				lock (_lobbyLock)
				{
					lobby.Theme = theme;
					Console.WriteLine($"[LobbyPresenter] Lobby {sessionId} theme updated to {theme}");
					return true;
				}
			}

			return false;
		}

		public int GetActiveLobbyCount()
		{
			return _lobbies.Count;
		}

		public int GetWaitingLobbyCount()
		{
			return _lobbies.Values.Count(l => l.Status == LobbyStatus.Waiting);
		}

		public Dictionary<string, object> GetLobbyStatistics()
		{
			return new Dictionary<string, object>
			{
				{ "TotalLobbies", _lobbies.Count },
				{ "WaitingLobbies", GetWaitingLobbyCount() },
				{ "InProgressLobbies", _lobbies.Values.Count(l => l.Status == LobbyStatus.InProgress) },
				{ "TotalPlayers", _lobbies.Values.Sum(l => l.CurrentPlayers) }
			};
		}
	}
}