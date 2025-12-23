namespace BombermanOnlineProject.Server.Patterns.Behavioral.Observer
{
	public interface IGameObserver
	{
		void OnPlayerKilled(string killerId, string victimId, int newScore);
		void OnEnemyKilled(string playerId, string enemyId, int scoreGained);
		void OnExplosionCreated(string ownerId, int centerX, int centerY, List<(int X, int Y)> affectedCells);
		void OnPowerUpCollected(string playerId, string powerUpType, int newValue);
		void OnBombPlaced(string playerId, int x, int y, int power);
		void OnWallDestroyed(int x, int y, string wallType, bool hadPowerUp);
		void OnGameStateChanged(string sessionId, string newState);
		string GetObserverName();
	}
}