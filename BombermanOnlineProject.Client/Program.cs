using BombermanOnlineProject.Client.Presenters;
using BombermanOnlineProject.Client.Views;
using BombermanOnlineProject.Server.Configuration;

var mainView = new ConsoleView();
var lobbyView = new LobbyView();
var leaderboardView = new LeaderboardView();
var gameView = new GameView(GameSettings.GameTheme.Forest);


var menuPresenter = new MenuPresenter(
	mainView,
	lobbyView,
	leaderboardView,
	gameView);

await menuPresenter.RunAsync();