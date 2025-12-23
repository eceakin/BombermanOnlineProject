using BombermanOnlineProject.Server.Core.Entities.Enemies;

namespace BombermanOnlineProject.Server.Patterns.Creational.Factory
{
	public class EnemyFactory : IEntityFactory<Enemy>
	{
		public const string STATIC = "Static";
		public const string CHASING = "Chasing";

		private readonly Random _random;
		private readonly Dictionary<string, Func<int, int, Enemy>> _enemyCreators;

		public EnemyFactory()
		{
			_random = new Random();

			_enemyCreators = new Dictionary<string, Func<int, int, Enemy>>(StringComparer.OrdinalIgnoreCase)
			{
				{ STATIC, (x, y) => new StaticEnemy(x, y) },
				{ CHASING, (x, y) => new ChasingEnemy(x, y) }
			};
		}

		public Enemy Create(string entityType, int x, int y)
		{
			if (string.IsNullOrWhiteSpace(entityType))
			{
				throw new ArgumentException("Entity type cannot be null or empty", nameof(entityType));
			}

			if (_enemyCreators.TryGetValue(entityType, out var creator))
			{
				var enemy = creator(x, y);
				Console.WriteLine($"[EnemyFactory] Created {entityType} enemy at ({x}, {y})");
				return enemy;
			}

			throw new ArgumentException($"Unknown enemy type: {entityType}", nameof(entityType));
		}

		public Enemy CreateRandom(int x, int y)
		{
			var types = new[] { STATIC, CHASING };
			var randomType = types[_random.Next(types.Length)];
			return Create(randomType, x, y);
		}

		public Enemy CreateWeightedRandom(int x, int y, float staticWeight = 0.6f, float chasingWeight = 0.4f)
		{
			float total = staticWeight + chasingWeight;
			float normalizedStatic = staticWeight / total;
			float roll = (float)_random.NextDouble();

			if (roll < normalizedStatic)
			{
				return Create(STATIC, x, y);
			}
			else
			{
				return Create(CHASING, x, y);
			}
		}

		public IEnumerable<string> GetAvailableTypes()
		{
			return _enemyCreators.Keys;
		}

		public bool IsValidEnemyType(string entityType)
		{
			return _enemyCreators.ContainsKey(entityType);
		}
	}
}