using BombermanOnlineProject.Server.Configuration;

namespace BombermanOnlineProject.Server.Core.Entities
{
	public class Player : GameObject
	{
		public string PlayerName { get; private set; }
		public int PlayerNumber { get; private set; }
		public float Speed { get; set; }
		public int BombPower { get; set; }
		public int MaxBombs { get; set; }
		public int ActiveBombs { get; set; }
		public bool IsInvulnerable { get; private set; }
		public DateTime InvulnerabilityEndTime { get; private set; }
		public int Score { get; set; }
		public int Kills { get; set; }
		public int Deaths { get; set; }
		public char DisplayChar { get; private set; }

		public Player(string playerName, int playerNumber, int startX, int startY)
			: base(startX, startY, GameObjectType.Player)
		{
			PlayerName = playerName;
			PlayerNumber = playerNumber;
			Speed = GameSettings.DefaultPlayerSpeed;
			BombPower = GameSettings.DefaultBombPower;
			MaxBombs = GameSettings.DefaultBombCount;
			ActiveBombs = 0;
			Score = 0;
			Kills = 0;
			Deaths = 0;
			DisplayChar = GetPlayerChar(playerNumber);
			IsInvulnerable = false;
		}

		public override void Update(float deltaTime)
		{
			if (IsInvulnerable && DateTime.UtcNow >= InvulnerabilityEndTime)
			{
				IsInvulnerable = false;
				Console.WriteLine($"[Player] {PlayerName} invulnerability ended");
			}
		}

		public bool CanPlaceBomb()
		{
			return ActiveBombs < MaxBombs && IsAlive;
		}

		public void PlaceBomb()
		{
			if (CanPlaceBomb())
			{
				ActiveBombs++;
			}
		}

		public void BombExploded()
		{
			if (ActiveBombs > 0)
			{
				ActiveBombs--;
			}
		}

		public bool MoveTo(int newX, int newY)
		{
			if (!IsAlive) return false;

			X = newX;
			Y = newY;
			return true;
		}

		public void TakeDamage()
		{
			if (IsInvulnerable || !IsAlive) return;

			Deaths++;
			Destroy();
			Console.WriteLine($"[Player] {PlayerName} died. Deaths: {Deaths}");
		}

		public void Respawn(int spawnX, int spawnY)
		{
			X = spawnX;
			Y = spawnY;
			IsAlive = true;
			ActiveBombs = 0;

			IsInvulnerable = true;
			InvulnerabilityEndTime = DateTime.UtcNow.AddMilliseconds(GameSettings.InvulnerabilityDuration);

			Console.WriteLine($"[Player] {PlayerName} respawned at ({spawnX}, {spawnY})");
		}

		public void CollectPowerUp(PowerUp powerUp)
		{
			switch (powerUp.PowerUpType)
			{
				case PowerUpType.SpeedBoost:
					Speed = Math.Min(Speed + GameSettings.SpeedBoostMultiplier, GameSettings.MaxPlayerSpeed);
					Console.WriteLine($"[Player] {PlayerName} speed increased to {Speed}");
					break;

				case PowerUpType.BombPowerIncrease:
					BombPower = Math.Min(BombPower + powerUp.Value, GameSettings.MaxBombPower);
					Console.WriteLine($"[Player] {PlayerName} bomb power increased to {BombPower}");
					break;

				case PowerUpType.BombCountIncrease:
					MaxBombs = Math.Min(MaxBombs + powerUp.Value, GameSettings.MaxBombCount);
					Console.WriteLine($"[Player] {PlayerName} max bombs increased to {MaxBombs}");
					break;
			}

			Score += 10;
		}

		public void AddKill()
		{
			Kills++;
			Score += 100;
		}

		private char GetPlayerChar(int playerNumber)
		{
			return playerNumber switch
			{
				0 => 'P',
				1 => 'Q',
				_ => '?'
			};
		}

		public override string ToString()
		{
			return $"Player {PlayerNumber} [{PlayerName}] at ({X}, {Y}) - HP: {(IsAlive ? "Alive" : "Dead")}, Score: {Score}";
		}
	}
}