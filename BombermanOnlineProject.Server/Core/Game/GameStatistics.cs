namespace BombermanOnlineProject.Server.Core.Game
{
	public class GameStatistics
	{
		public string SessionId { get; set; } = string.Empty;
		public int PlayerCount { get; set; }
		public int CurrentRound { get; set; }
		public int ActiveBombs { get; set; }
		public int ActiveExplosions { get; set; }
		public int ActiveEnemies { get; set; }
		public int ActivePowerUps { get; set; }
		public GameState State { get; set; }
		public TimeSpan Duration { get; set; }
	}

}