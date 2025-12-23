namespace BombermanOnlineProject.Server.Patterns.Behavioral.Observer
{
	public class GameSubject
	{
		private readonly List<IGameObserver> _observers;
		private readonly object _observerLock;
		private readonly string _sessionId;

		public GameSubject(string sessionId)
		{
			_observers = new List<IGameObserver>();
			_observerLock = new object();
			_sessionId = sessionId;
		}

		public void Attach(IGameObserver observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			lock (_observerLock)
			{
				if (!_observers.Contains(observer))
				{
					_observers.Add(observer);
					Console.WriteLine($"[GameSubject] Observer '{observer.GetObserverName()}' attached to session {_sessionId}");
				}
			}
		}

		public void Detach(IGameObserver observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			lock (_observerLock)
			{
				if (_observers.Remove(observer))
				{
					Console.WriteLine($"[GameSubject] Observer '{observer.GetObserverName()}' detached from session {_sessionId}");
				}
			}
		}

		public void NotifyPlayerKilled(string killerId, string victimId, int newScore)
		{
			lock (_observerLock)
			{
				foreach (var observer in _observers.ToList())
				{
					try
					{
						observer.OnPlayerKilled(killerId, victimId, newScore);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[GameSubject] Error notifying {observer.GetObserverName()}: {ex.Message}");
					}
				}
			}
		}

		public void NotifyEnemyKilled(string playerId, string enemyId, int scoreGained)
		{
			lock (_observerLock)
			{
				foreach (var observer in _observers.ToList())
				{
					try
					{
						observer.OnEnemyKilled(playerId, enemyId, scoreGained);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[GameSubject] Error notifying {observer.GetObserverName()}: {ex.Message}");
					}
				}
			}
		}

		public void NotifyExplosionCreated(string ownerId, int centerX, int centerY, List<(int X, int Y)> affectedCells)
		{
			lock (_observerLock)
			{
				foreach (var observer in _observers.ToList())
				{
					try
					{
						observer.OnExplosionCreated(ownerId, centerX, centerY, affectedCells);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[GameSubject] Error notifying {observer.GetObserverName()}: {ex.Message}");
					}
				}
			}
		}

		public void NotifyPowerUpCollected(string playerId, string powerUpType, int newValue)
		{
			lock (_observerLock)
			{
				foreach (var observer in _observers.ToList())
				{
					try
					{
						observer.OnPowerUpCollected(playerId, powerUpType, newValue);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[GameSubject] Error notifying {observer.GetObserverName()}: {ex.Message}");
					}
				}
			}
		}

		public void NotifyBombPlaced(string playerId, int x, int y, int power)
		{
			lock (_observerLock)
			{
				foreach (var observer in _observers.ToList())
				{
					try
					{
						observer.OnBombPlaced(playerId, x, y, power);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[GameSubject] Error notifying {observer.GetObserverName()}: {ex.Message}");
					}
				}
			}
		}

		public void NotifyWallDestroyed(int x, int y, string wallType, bool hadPowerUp)
		{
			lock (_observerLock)
			{
				foreach (var observer in _observers.ToList())
				{
					try
					{
						observer.OnWallDestroyed(x, y, wallType, hadPowerUp);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[GameSubject] Error notifying {observer.GetObserverName()}: {ex.Message}");
					}
				}
			}
		}

		public void NotifyGameStateChanged(string newState)
		{
			lock (_observerLock)
			{
				foreach (var observer in _observers.ToList())
				{
					try
					{
						observer.OnGameStateChanged(_sessionId, newState);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[GameSubject] Error notifying {observer.GetObserverName()}: {ex.Message}");
					}
				}
			}
		}

		public int GetObserverCount()
		{
			lock (_observerLock)
			{
				return _observers.Count;
			}
		}

		public void ClearObservers()
		{
			lock (_observerLock)
			{
				_observers.Clear();
				Console.WriteLine($"[GameSubject] All observers cleared from session {_sessionId}");
			}
		}
	}
}