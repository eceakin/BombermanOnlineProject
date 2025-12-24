using System;

namespace BombermanOnlineProject.Client.Views
{
	public class ConsoleView : IView
	{
		public void Show()
		{
			// Konsol başlığını ayarlar ve ekranı hazırlar
			Console.Title = "Bomberman Online";
			Console.CursorVisible = true;
		}

		public void Hide()
		{
			// Uygulama kapanırken veya gizlenirken yapılacak işlemler
			Console.Clear();
			Console.WriteLine("Closing application...");
		}

		public void Clear()
		{
			Console.Clear();
		}

		public void DisplayMessage(string message)
		{
			Console.WriteLine(message);
		}

		public void DisplayError(string error)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"\n[ERROR] {error}");
			Console.ResetColor();
		}

		public void DisplaySuccess(string message)
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"\n[SUCCESS] {message}");
			Console.ResetColor();
		}

		public bool ConfirmAction(string message)
		{
			Console.Write($"{message} (Y/N): ");
			var key = Console.ReadKey(intercept: true).Key;
			Console.WriteLine(); // Alt satıra geçmek için
			return key == ConsoleKey.Y;
		}

		public string GetUserInput(string prompt)
		{
			Console.Write($"{prompt}: ");
			return Console.ReadLine() ?? string.Empty;
		}
	}
}