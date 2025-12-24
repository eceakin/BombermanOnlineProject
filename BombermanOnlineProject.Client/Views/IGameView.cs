using BombermanOnlineProject.Server.Core.Map;

namespace BombermanOnlineProject.Client.Views
{
	public interface IGameView : IView
	{
		void RenderMap(GameMap map, Dictionary<string, (int X, int Y, bool IsAlive)> players,
					   Dictionary<string, (int X, int Y)> bombs,
					   Dictionary<string, List<(int X, int Y)>> explosions);

		void DisplayPlayerStats(string playerId, int score, int kills, int deaths,
							   float speed, int bombPower, int maxBombs, int activeBombs);

		void DisplayGameInfo(string sessionId, int currentRound, int playerCount,
							string gameState, TimeSpan duration);

		void DisplayControls();

		void ShowGameStarted();

		void ShowGameEnded(string winnerId, Dictionary<string, int> finalScores);

		void ShowRoundWon(string winnerId, int roundNumber);

		void ShowPlayerJoined(string playerId, string playerName);

		void ShowPlayerLeft(string playerId);

		void ShowBombPlaced(string playerId, int x, int y);

		void ShowExplosion(int centerX, int centerY, List<(int X, int Y)> affectedCells);

		void ShowPowerUpCollected(string playerId, string powerUpType);

		void ShowPlayerDied(string playerId);

		void UpdateGameState(object gameState);

		ConsoleKey WaitForInput();
	}
}