using BombermanOnlineProject.Server.Configuration;

namespace BombermanOnlineProject.Server.Core.Entities
{
	public class Bomb : GameObject
	{
		public string OwnerId { get; private set; }
		public int Power { get; private set; }
		public DateTime PlacedAt { get; private set; }
		public int TimerMilliseconds { get; private set; }
		public bool HasExploded { get; private set; }
		public char DisplayChar { get; private set; }

		public Bomb(string ownerId, int x, int y, int power, int timerMs = -1)
			: base(x, y, GameObjectType.Bomb)
		{
			OwnerId = ownerId;
			Power = power;
			PlacedAt = DateTime.UtcNow;
			TimerMilliseconds = timerMs > 0 ? timerMs : GameSettings.BombTimer;
			HasExploded = false;
			DisplayChar = '●';
		}

		public override void Update(float deltaTime)
		{
			if (!IsAlive || HasExploded) return;

			var elapsed = (DateTime.UtcNow - PlacedAt).TotalMilliseconds;
			if (elapsed >= TimerMilliseconds)
			{
				Explode();
			}
		}

		public void Explode()
		{
			if (HasExploded) return;

			HasExploded = true;
			IsAlive = false;
			Console.WriteLine($"[Bomb] Bomb at ({X}, {Y}) exploded with power {Power}");
		}

		public float GetRemainingTimePercentage()
		{
			var elapsed = (DateTime.UtcNow - PlacedAt).TotalMilliseconds;
			return Math.Max(0, 1.0f - (float)(elapsed / TimerMilliseconds));
		}

		public int GetRemainingMilliseconds()
		{
			var elapsed = (DateTime.UtcNow - PlacedAt).TotalMilliseconds;
			return Math.Max(0, (int)(TimerMilliseconds - elapsed));
		}

		public bool ShouldExplode()
		{
			var elapsed = (DateTime.UtcNow - PlacedAt).TotalMilliseconds;
			return elapsed >= TimerMilliseconds && !HasExploded;
		}

		public override string ToString()
		{
			var remaining = GetRemainingMilliseconds();
			return $"Bomb [Owner: {OwnerId.Substring(0, 8)}...] at ({X}, {Y}) - Power: {Power}, Remaining: {remaining}ms";
		}
	}
}