namespace BombermanOnlineProject.Client.Views
{
	public class LobbyInfo
	{
		public string SessionId { get; set; } = string.Empty;
		public string HostPlayerName { get; set; } = string.Empty;
		public int CurrentPlayers { get; set; }
		public int MaxPlayers { get; set; }
		public string Theme { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public string Status { get; set; } = string.Empty;
	}
}