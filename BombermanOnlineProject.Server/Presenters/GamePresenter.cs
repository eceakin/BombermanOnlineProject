using BombermanOnlineProject.Server.Core.Game;
using BombermanOnlineProject.Server.Services;

namespace BombermanOnlineProject.Server.Presenters
{
	public class GamePresenter
	{
		private readonly StatisticsService _statisticsService;
		private readonly LeaderboardService _leaderboardService;

		public GamePresenter(StatisticsService statisticsService, LeaderboardService leaderboardService)
		{
			_statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
			_leaderboardService = leaderboardService ?? throw new ArgumentNullException(nameof(leaderboardService));
		}

		public async Task<GameSession?> CreateGameSession(string hostPlayerId)
		{
			try
			{
				var session = GameManager.Instance.CreateGameSession(hostPlayerId);
				Console.WriteLine($"[GamePresenter] Game session created: {session.SessionId}");
				return session;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error creating game session: {ex.Message}");
				return null;
			}
		}

		public GameSession? GetGameSession(string sessionId)
		{
			return GameManager.Instance.GetGameSession(sessionId);
		}

		public bool JoinGameSession(string sessionId, string playerId)
		{
			try
			{
				return GameManager.Instance.JoinGameSession(sessionId, playerId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error joining game session: {ex.Message}");
				return false;
			}
		}

		public bool LeaveGameSession(string playerId)
		{
			try
			{
				return GameManager.Instance.LeaveGameSession(playerId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error leaving game session: {ex.Message}");
				return false;
			}
		}

		public bool StartGame(string sessionId)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					return false;
				}

				session.StartGame();
				Console.WriteLine($"[GamePresenter] Game started: {sessionId}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error starting game: {ex.Message}");
				return false;
			}
		}

		public bool PauseGame(string sessionId)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					return false;
				}

				session.PauseGame();
				Console.WriteLine($"[GamePresenter] Game paused: {sessionId}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error pausing game: {ex.Message}");
				return false;
			}
		}

		public bool ResumeGame(string sessionId)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					return false;
				}

				session.ResumeGame();
				Console.WriteLine($"[GamePresenter] Game resumed: {sessionId}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error resuming game: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> EndGameAndRecordStatistics(string sessionId, int userId, string opponentUsername)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					return false;
				}

				var player = session.GetPlayerIds()
					.Select(id => session.GetPlayer(id))
					.FirstOrDefault(p => p != null);

				if (player == null)
				{
					return false;
				}

				session.EndGame();

				var roundWins = session.GetRoundWins();
				var isWinner = roundWins.ContainsKey(player.Id) &&
							   roundWins[player.Id] >= Configuration.GameSettings.RoundsToWin;

				var stats = session.GetGameStatistics();

				await _statisticsService.RecordGameStatisticAsync(
					userId: userId,
					sessionId: sessionId,
					isWinner: isWinner,
					finalScore: player.Score,
					kills: player.Kills,
					deaths: player.Deaths,
					bombsPlaced: 0,
					wallsDestroyed: 0,
					powerUpsCollected: 0,
					roundsWon: roundWins.ContainsKey(player.Id) ? roundWins[player.Id] : 0,
					roundsLost: session.CurrentRound - (roundWins.ContainsKey(player.Id) ? roundWins[player.Id] : 0),
					gameDuration: stats.Duration,
					opponentUsername: opponentUsername,
					mapTheme: session.Map.Theme.ToString()
				);

				if (await _leaderboardService.IsNewHighScoreAsync(userId, player.Score))
				{
					await _leaderboardService.RecordHighScoreAsync(
						userId: userId,
						sessionId: sessionId,
						score: player.Score,
						kills: player.Kills,
						deaths: player.Deaths,
						gameDuration: stats.Duration,
						roundsWon: roundWins.ContainsKey(player.Id) ? roundWins[player.Id] : 0,
						opponentUsername: opponentUsername,
						mapTheme: session.Map.Theme.ToString()
					);
				}

				GameManager.Instance.RemoveGameSession(sessionId);
				Console.WriteLine($"[GamePresenter] Game ended and statistics recorded: {sessionId}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error ending game: {ex.Message}");
				return false;
			}
		}

		public bool MovePlayer(string sessionId, string playerId, int newX, int newY)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					return false;
				}

				return session.MovePlayer(playerId, newX, newY);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error moving player: {ex.Message}");
				return false;
			}
		}

		public bool PlaceBomb(string sessionId, string playerId)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				if (session == null)
				{
					return false;
				}

				return session.PlaceBomb(playerId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error placing bomb: {ex.Message}");
				return false;
			}
		}

		public GameStatistics? GetGameStatistics(string sessionId)
		{
			try
			{
				var session = GameManager.Instance.GetGameSession(sessionId);
				return session?.GetGameStatistics();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GamePresenter] Error getting game statistics: {ex.Message}");
				return null;
			}
		}

		public List<GameSession> GetAllActiveSessions()
		{
			return GameManager.Instance.GetAllActiveSessions().ToList();
		}

		public (int ActiveSessions, int TotalGamesCreated, int ConnectedPlayers) GetGlobalStatistics()
		{
			return GameManager.Instance.GetStatistics();
		}
	}
}