using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Map;

namespace BombermanOnlineProject.Server.Core.Entities
{
	public class Explosion : GameObject
	{
		public string OwnerId { get; private set; }
		public DateTime CreatedAt { get; private set; }
		public int DurationMilliseconds { get; private set; }
		public List<(int X, int Y)> AffectedCells { get; private set; }
		public char DisplayChar { get; private set; }

		public Explosion(string ownerId, int centerX, int centerY, int power, GameMap map, int durationMs = -1)
			: base(centerX, centerY, GameObjectType.Explosion)
		{
			OwnerId = ownerId;
			CreatedAt = DateTime.UtcNow;
			DurationMilliseconds = durationMs > 0 ? durationMs : GameSettings.ExplosionDuration;
			DisplayChar = '※';
			AffectedCells = new List<(int, int)>();

			CalculateExplosionArea(centerX, centerY, power, map);
		}

		private void CalculateExplosionArea(int centerX, int centerY, int power, GameMap map)
		{
			AffectedCells.Add((centerX, centerY));

			var directions = new[]
			{
				(0, -1),  // Up
				(0, 1),   // Down
				(-1, 0),  // Left
				(1, 0)    // Right
			};

			foreach (var (dx, dy) in directions)
			{
				for (int i = 1; i <= power; i++)
				{
					int newX = centerX + (dx * i);
					int newY = centerY + (dy * i);

					if (!map.IsValidPosition(newX, newY))
						break;

					var cell = map.GetCell(newX, newY);

					if (cell.HasUnbreakableWall())
						break;

					AffectedCells.Add((newX, newY));

					if (cell.HasWall() && cell.Wall!.IsBreakable)
					{
						break;
					}
				}
			}

			Console.WriteLine($"[Explosion] Created at ({centerX}, {centerY}) affecting {AffectedCells.Count} cells");
		}

		public override void Update(float deltaTime)
		{
			if (!IsAlive) return;

			var elapsed = (DateTime.UtcNow - CreatedAt).TotalMilliseconds;
			if (elapsed >= DurationMilliseconds)
			{
				Destroy();
			}
		}

		public bool IsExpired()
		{
			var elapsed = (DateTime.UtcNow - CreatedAt).TotalMilliseconds;
			return elapsed >= DurationMilliseconds;
		}

		public bool AffectsPosition(int x, int y)
		{
			return AffectedCells.Contains((x, y));
		}

		public override string ToString()
		{
			return $"Explosion [Owner: {OwnerId.Substring(0, 8)}...] at ({X}, {Y}) - Cells: {AffectedCells.Count}";
		}
	}
}