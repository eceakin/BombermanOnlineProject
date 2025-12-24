namespace BombermanOnlineProject.Server.Presenters
{
	public class LobbyInfo
	{
		public string SessionId { get; set; } = string.Empty;
		public string HostPlayerId { get; set; } = string.Empty;
		public string HostPlayerName { get; set; } = string.Empty;
		public List<string> PlayerIds { get; set; } = new List<string>();
		public List<string> PlayerNames { get; set; } = new List<string>();
		public string Theme { get; set; } = "Forest";
		public DateTime CreatedAt { get; set; }
		public DateTime? StartedAt { get; set; }
		public LobbyStatus Status { get; set; }
		public int MaxPlayers { get; set; }
		public int CurrentPlayers { get; set; }
	}
}