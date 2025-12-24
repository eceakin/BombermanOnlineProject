namespace BombermanOnlineProject.Client.Views
{
	public class UserStatsInfo
	{
		public string Username { get; set; } = string.Empty;
		public int TotalGames { get; set; }
		public int TotalWins { get; set; }
		public int TotalLosses { get; set; }
		public int TotalKills { get; set; }
		public int TotalDeaths { get; set; }
		public int HighestScore { get; set; }
		public double WinRate { get; set; }
		public double KDRatio { get; set; }
		public double AverageScore { get; set; }
	}
}