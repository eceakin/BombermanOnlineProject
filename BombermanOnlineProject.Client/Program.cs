using BombermanOnlineProject.Client.Presenters;
using BombermanOnlineProject.Client.Views;
using System.Text;

// 1. Konsol Ayarları: ASCII ve Kutu çizim karakterleri için UTF8 desteği
Console.OutputEncoding = Encoding.UTF8;
Console.Title = "Bomberman Online Client";

// 2. View Katmanı Başlatma (MVP - View Layer)
// Bu sınıflar sadece ekrana çizim yapmaktan sorumludur.
var mainView = new ConsoleView();
var lobbyView = new LobbyView();
var leaderboardView = new LeaderboardView();
var gameView = new GameView(); // Varsayılan olarak Forest temasıyla başlar

// 3. Presenter Katmanı Başlatma (MVP - Presenter Layer)
// MenuPresenter, tüm menü akışını ve View'lar arası geçişi yöneten "beyin"dir.
var menuPresenter = new MenuPresenter(
	mainView,
	lobbyView,
	leaderboardView,
	gameView);

try
{
	// 4. Uygulama Döngüsünü Başlat
	// Bu metod kullanıcı "Exit" diyene kadar programı açık tutar.
	await menuPresenter.RunAsync();
}
catch (Exception ex)
{
	Console.ForegroundColor = ConsoleColor.Red;
	Console.WriteLine($"\n[CRITICAL ERROR] Application failed to start: {ex.Message}");
	Console.ResetColor();
	Console.WriteLine("Press any key to exit...");
	Console.ReadKey();
}