using BombermanOnlineProject.Client.Presenters;
using BombermanOnlineProject.Client.Views;

var mainView = new ConsoleView();
var lobbyView = new LobbyView();
var leaderboardView = new LeaderboardView();
var gameView = new GameView();

var menuPresenter = new MenuPresenter(
	mainView,
	lobbyView,
	leaderboardView,
	gameView);

await menuPresenter.RunAsync();