using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Entities;

namespace BombermanOnlineProject.Server.Patterns.Creational.Factory
{
	public class PowerUpFactory : IEntityFactory<PowerUp>
	{
		public const string SPEED_BOOST = "SpeedBoost";
		public const string BOMB_POWER = "BombPower";
		public const string BOMB_COUNT = "BombCount";

		private readonly Random _random;
		private readonly Dictionary<string, Func<int, int, PowerUp>> _powerUpCreators;

		public PowerUpFactory()
		{
			_random = new Random();

			_powerUpCreators = new Dictionary<string, Func<int, int, PowerUp>>(StringComparer.OrdinalIgnoreCase)
			{
				{ SPEED_BOOST, (x, y) => new PowerUp(x, y, PowerUpType.SpeedBoost, value: 1) },
				{ BOMB_POWER, (x, y) => new PowerUp(x, y, PowerUpType.BombPowerIncrease, value: GameSettings.BombPowerIncrement) },
				{ BOMB_COUNT, (x, y) => new PowerUp(x, y, PowerUpType.BombCountIncrease, value: GameSettings.BombCountIncrement) }
			};
		}

		public PowerUp Create(string entityType, int x, int y)
		{
			if (string.IsNullOrWhiteSpace(entityType))
			{
				throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));
			}

			if (_powerUpCreators.TryGetValue(entityType, out var creator))
			{
				var powerUp = creator(x, y);
				Console.WriteLine($"[PowerUpFactory] Created {entityType} at ({x}, {y})");
				return powerUp;
			}

			throw new ArgumentException($"Unknown power-up type: {entityType}", nameof(entityType));
		}

		public PowerUp CreateRandom(int x, int y)
		{
			var types = new[] { SPEED_BOOST, BOMB_POWER, BOMB_COUNT };
			var randomType = types[_random.Next(types.Length)];
			return Create(randomType, x, y);
		}

		public PowerUp CreateWeightedRandom(int x, int y,
			float speedWeight = 0.3f,
			float bombPowerWeight = 0.4f,
			float bombCountWeight = 0.3f)
		{
			float total = speedWeight + bombPowerWeight + bombCountWeight;
			float normalizedSpeed = speedWeight / total;
			float normalizedBombPower = bombPowerWeight / total;

			float roll = (float)_random.NextDouble();

			if (roll < normalizedSpeed)
			{
				return Create(SPEED_BOOST, x, y);
			}
			else if (roll < normalizedSpeed + normalizedBombPower)
			{
				return Create(BOMB_POWER, x, y);
			}
			else
			{
				return Create(BOMB_COUNT, x, y);
			}
		}

		public IEnumerable<string> GetAvailableTypes()
		{
			return _powerUpCreators.Keys;
		}

		public bool IsValidPowerUpType(string entityType)
		{
			return _powerUpCreators.ContainsKey(entityType);
		}
	}
}