namespace BombermanOnlineProject.Client.Views
{
	public interface ILobbyView : IView
	{
		void DisplayLobbyList(List<LobbyInfo> lobbies);

		void DisplayLobbyDetails(LobbyInfo lobby);

		void ShowLobbyCreated(string sessionId);

		void ShowPlayerJoinedLobby(string playerName);

		void ShowPlayerLeftLobby(string playerName);

		void ShowLobbyFull();

		void ShowLobbyNotFound();

		void ShowWaitingForPlayers(int currentPlayers, int maxPlayers);

		void ShowLobbyStarting();

		void DisplayLobbyMenu();

		int GetMenuChoice();

		string GetSessionIdInput();

		string GetPlayerNameInput();

		string GetThemeChoice();
	}
}