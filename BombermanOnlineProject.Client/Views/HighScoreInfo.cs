namespace BombermanOnlineProject.Client.Views
{
	public class HighScoreInfo
	{
		public string Username { get; set; } = string.Empty;
		public int Score { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public DateTime AchievedAt { get; set; }
		public string MapTheme { get; set; } = string.Empty;
	}
}