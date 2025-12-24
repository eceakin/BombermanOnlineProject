namespace BombermanOnlineProject.Client.Views
{
	public interface IView
	{
		void Show();
		void Hide();
		void Clear();
		void DisplayMessage(string message);
		void DisplayError(string error);
		void DisplaySuccess(string message);
		bool ConfirmAction(string message);
		string GetUserInput(string prompt);
	}
}