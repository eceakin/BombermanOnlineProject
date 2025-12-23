namespace BombermanOnlineProject.Server.Core.Entities
{
	public class PowerUp : GameObject
	{
		public PowerUpType PowerUpType { get; private set; }
		public int Value { get; private set; }
		public char DisplayChar { get; private set; }
		public DateTime SpawnedAt { get; private set; }
		public int LifetimeSeconds { get; private set; }

		public PowerUp(int x, int y, PowerUpType type, int value = 1, int lifetimeSeconds = 30)
			: base(x, y, GameObjectType.PowerUp)
		{
			PowerUpType = type;
			Value = value;
			LifetimeSeconds = lifetimeSeconds;
			SpawnedAt = DateTime.UtcNow;
			DisplayChar = GetDisplayCharForType(type);
		}

		public override void Update(float deltaTime)
		{
			var elapsed = (DateTime.UtcNow - SpawnedAt).TotalSeconds;
			if (elapsed >= LifetimeSeconds)
			{
				Destroy();
			}
		}

		public bool IsExpired()
		{
			var elapsed = (DateTime.UtcNow - SpawnedAt).TotalSeconds;
			return elapsed >= LifetimeSeconds;
		}

		public float GetRemainingLifetimePercentage()
		{
			var elapsed = (DateTime.UtcNow - SpawnedAt).TotalSeconds;
			return Math.Max(0, 1.0f - (float)(elapsed / LifetimeSeconds));
		}

		private char GetDisplayCharForType(PowerUpType type)
		{
			return type switch
			{
				PowerUpType.SpeedBoost => '→',
				PowerUpType.BombPowerIncrease => '☼',
				PowerUpType.BombCountIncrease => '◙',
				_ => '?'
			};
		}

		public override string ToString()
		{
			return $"PowerUp [{PowerUpType}] at ({X}, {Y}) - Value: {Value}";
		}
	}
}