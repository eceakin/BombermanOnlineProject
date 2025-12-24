using System.Collections.Concurrent;

namespace BombermanOnlineProject.Server.Core.Game
{
	/// <summary>
	/// SINGLETON PATTERN Implementation
	/// 
	/// Purpose: Ensures only ONE instance of GameManager exists throughout the application.
	/// This is critical for managing global game state, active sessions, and player connections.
	/// 
	/// Why Singleton here?
	/// 1. Global Access: All parts of the application need to access active games
	/// 2. State Consistency: Single source of truth for all game sessions
	/// 3. Resource Management: Prevents multiple managers competing for resources
	/// 4. Thread Safety: Centralized synchronization point
	/// 
	/// Implementation Details:
	/// - Thread-safe using Lazy<T> (lazy initialization)
	/// - Private constructor prevents external instantiation
	/// - ConcurrentDictionary for thread-safe game session management
	/// </summary>
	public sealed class GameManager
	{
		#region Singleton Implementation

		/// <summary>
		/// Lazy initialization ensures thread-safety without explicit locking.
		/// The instance is created only when first accessed.
		/// 'sealed' keyword prevents inheritance which could break singleton pattern.
		/// </summary>
		private static readonly Lazy<GameManager> _instance =
			new Lazy<GameManager>(() => new GameManager());

		/// <summary>
		/// Public access point to the singleton instance
		/// </summary>
		public static GameManager Instance => _instance.Value;

		/// <summary>
		/// Private constructor prevents direct instantiation from outside.
		/// This is a core requirement of the Singleton pattern.
		/// </summary>
		private GameManager()
		{
			_activeSessions = new ConcurrentDictionary<string, GameSession>();
			_playerToSessionMap = new ConcurrentDictionary<string, string>();

			Console.WriteLine("[GameManager] Singleton instance initialized");
		}

		#endregion

		#region Game Session Management

		/// <summary>
		/// Thread-safe dictionary storing all active game sessions.
		/// Key: SessionId (GUID), Value: GameSession instance
		/// ConcurrentDictionary chosen for thread-safe operations in multiplayer environment.
		/// </summary>
		private readonly ConcurrentDictionary<string, GameSession> _activeSessions;

		/// <summary>
		/// Maps player IDs to their current session IDs for quick lookup.
		/// Prevents players from joining multiple games simultaneously.
		/// </summary>
		private readonly ConcurrentDictionary<string, string> _playerToSessionMap;

		/// <summary>
		/// Total number of games created since server start (for statistics)
		/// </summary>
		private int _totalGamesCreated = 0;

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates a new game session with a unique identifier
		/// </summary>
		/// <param name="hostPlayerId">ID of the player hosting the game</param>
		/// <returns>Newly created GameSession instance</returns>
		public GameSession CreateGameSession(string hostPlayerId)
		{
			if (string.IsNullOrWhiteSpace(hostPlayerId))
			{
				throw new ArgumentException("Host player ID cannot be null or empty", nameof(hostPlayerId));
			}

			// Check if player is already in a game
			if (_playerToSessionMap.ContainsKey(hostPlayerId))
			{
				throw new InvalidOperationException($"Player {hostPlayerId} is already in a game session");
			}

			string sessionId = Guid.NewGuid().ToString();
			var gameSession = new GameSession(sessionId, hostPlayerId);

			// Thread-safe add operation
			if (_activeSessions.TryAdd(sessionId, gameSession))
			{
				_playerToSessionMap.TryAdd(hostPlayerId, sessionId);
				_totalGamesCreated++;

				Console.WriteLine($"[GameManager] New game session created: {sessionId} by player {hostPlayerId}");
				return gameSession;
			}

			throw new InvalidOperationException("Failed to create game session");
		}

		/// <summary>
		/// Retrieves a game session by its ID
		/// </summary>
		/// <param name="sessionId">Unique session identifier</param>
		/// <returns>GameSession if found, null otherwise</returns>
		public GameSession? GetGameSession(string sessionId)
		{
			_activeSessions.TryGetValue(sessionId, out var session);
			return session;
		}

		/// <summary>
		/// Gets the session ID for a specific player
		/// </summary>
		/// <param name="playerId">Player identifier</param>
		/// <returns>Session ID if player is in a game, null otherwise</returns>
		public string? GetPlayerSession(string playerId)
		{
			_playerToSessionMap.TryGetValue(playerId, out var sessionId);
			return sessionId;
		}

		/// <summary>
		/// Removes a game session and cleans up player mappings
		/// </summary>
		/// <param name="sessionId">Session to remove</param>
		/// <returns>True if session was removed successfully</returns>
		public bool RemoveGameSession(string sessionId)
		{
			if (_activeSessions.TryRemove(sessionId, out var removedSession))
			{
				// Clean up player mappings
				foreach (var playerId in removedSession.GetPlayerIds())
				{
					_playerToSessionMap.TryRemove(playerId, out _);
				}

				Console.WriteLine($"[GameManager] Game session removed: {sessionId}");
				return true;
			}

			return false;
		}

		/// <summary>
		/// Adds a player to an existing game session
		/// </summary>
		/// <param name="sessionId">Target session ID</param>
		/// <param name="playerId">Player to add</param>
		/// <returns>True if player was added successfully</returns>
		public bool JoinGameSession(string sessionId, string playerId)
		{
			if (string.IsNullOrWhiteSpace(playerId))
			{
				throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
			}

			// Check if player is already in a game
			if (_playerToSessionMap.ContainsKey(playerId))
			{
				Console.WriteLine($"[GameManager] Player {playerId} is already in a game");
				return false;
			}

			var session = GetGameSession(sessionId);
			if (session == null)
			{
				Console.WriteLine($"[GameManager] Session {sessionId} not found");
				return false;
			}

			// Attempt to add player to session (GameSession will handle capacity checks)
			if (session.AddPlayer(playerId))
			{
				_playerToSessionMap.TryAdd(playerId, sessionId);
				Console.WriteLine($"[GameManager] Player {playerId} joined session {sessionId}");
				return true;
			}

			return false;
		}

		/// <summary>
		/// Removes a player from their current session
		/// </summary>
		/// <param name="playerId">Player to remove</param>
		/// <returns>True if player was removed successfully</returns>
		public bool LeaveGameSession(string playerId)
		{
			if (_playerToSessionMap.TryRemove(playerId, out var sessionId))
			{
				var session = GetGameSession(sessionId);
				if (session != null)
				{
					session.RemovePlayer(playerId);
					Console.WriteLine($"[GameManager] Player {playerId} left session {sessionId}");

					// If session is empty, remove it
					if (session.IsEmpty())
					{
						RemoveGameSession(sessionId);
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets all currently active game sessions
		/// </summary>
		/// <returns>List of active GameSession instances</returns>
		public IReadOnlyList<GameSession> GetAllActiveSessions()
		{
			return _activeSessions.Values.ToList();
		}

		/// <summary>
		/// Gets statistics about the game manager
		/// </summary>
		public (int ActiveSessions, int TotalGamesCreated, int ConnectedPlayers) GetStatistics()
		{
			return (
				ActiveSessions: _activeSessions.Count,
				TotalGamesCreated: _totalGamesCreated,
				ConnectedPlayers: _playerToSessionMap.Count
			);
		}

		#endregion

		#region Cleanup Methods

		/// <summary>
		/// Clears all game sessions (useful for testing or server shutdown)
		/// </summary>
		public void ClearAllSessions()
		{
			Console.WriteLine("[GameManager] Clearing all game sessions...");
			_activeSessions.Clear();
			_playerToSessionMap.Clear();
		}

		#endregion
	}

	/// <summary>
	/// Placeholder for GameSession class (will be implemented in FASE 5)
	/// This is a stub to allow GameManager to compile
	/// </summary>
	
}