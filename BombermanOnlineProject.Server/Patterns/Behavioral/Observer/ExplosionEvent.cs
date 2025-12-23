namespace BombermanOnlineProject.Server.Patterns.Behavioral.Observer
{
	public class ExplosionEvent
	{
		public string OwnerId { get; set; } = string.Empty;
		public int CenterX { get; set; }
		public int CenterY { get; set; }
		public int AffectedCellsCount { get; set; }
		public DateTime Timestamp { get; set; }

		public override string ToString()
		{
			return $"Explosion at ({CenterX}, {CenterY}) by {OwnerId.Substring(0, 8)} - " +
				   $"{AffectedCellsCount} cells affected at {Timestamp:HH:mm:ss}";
		}
	}
}