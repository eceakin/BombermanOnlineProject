namespace BombermanOnlineProject.Client.Views
{
	public interface ILeaderboardView : IView
	{
		void DisplayTopPlayersByScore(List<PlayerRankInfo> players);

		void DisplayTopPlayersByWins(List<PlayerRankInfo> players);

		void DisplayTopPlayersByKills(List<PlayerRankInfo> players);

		void DisplayUserStats(UserStatsInfo stats);

		void DisplayUserRank(int scoreRank, int winsRank, int killsRank);

		void DisplayRecentHighScores(List<HighScoreInfo> highScores);

		void DisplayLeaderboardMenu();

		int GetMenuChoice();

		string GetUsernameInput();
	}
}