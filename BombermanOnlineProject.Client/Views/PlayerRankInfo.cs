namespace BombermanOnlineProject.Client.Views
{
	public class PlayerRankInfo
	{
		public int Rank { get; set; }
		public string Username { get; set; } = string.Empty;
		public int Score { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public double WinRate { get; set; }
		public double KDRatio { get; set; }
	}
}