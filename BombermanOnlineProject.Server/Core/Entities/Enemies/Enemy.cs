using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Map;
using BombermanOnlineProject.Server.Patterns.Behavioral.Strategy;

namespace BombermanOnlineProject.Server.Core.Entities.Enemies
{
	public abstract class Enemy : GameObject
	{
		public string EnemyName { get; protected set; }
		public float Speed { get; protected set; }
		public int ScoreValue { get; protected set; }
		public char DisplayChar { get; protected set; }
		public IMovementStrategy MovementStrategy { get; set; }

		private DateTime _lastMoveTime;
		private int _moveIntervalMs;

		protected Enemy(int x, int y, string name, float speed, int scoreValue, char displayChar, IMovementStrategy strategy)
			: base(x, y, GameObjectType.Enemy)
		{
			EnemyName = name;
			Speed = speed;
			ScoreValue = scoreValue;
			DisplayChar = displayChar;
			MovementStrategy = strategy;
			_lastMoveTime = DateTime.UtcNow;
			_moveIntervalMs = GameSettings.EnemyAIUpdateInterval;
		}

		public override void Update(float deltaTime)
		{
			if (!IsAlive) return;

			var elapsed = (DateTime.UtcNow - _lastMoveTime).TotalMilliseconds;
			if (elapsed >= _moveIntervalMs)
			{
				_lastMoveTime = DateTime.UtcNow;
			}
		}

		public (int X, int Y) GetNextMove(GameMap map, int? targetX = null, int? targetY = null)
		{
			return MovementStrategy.GetNextMove(X, Y, map, targetX, targetY);
		}

		public bool MoveTo(int newX, int newY, GameMap map)
		{
			if (!IsAlive) return false;

			if (!map.IsWalkable(newX, newY))
				return false;

			X = newX;
			Y = newY;
			return true;
		}

		public void TakeDamage()
		{
			Destroy();
			Console.WriteLine($"[Enemy] {EnemyName} at ({X}, {Y}) destroyed. Score: {ScoreValue}");
		}

		public override string ToString()
		{
			return $"Enemy [{EnemyName}] at ({X}, {Y}) - Strategy: {MovementStrategy.GetStrategyName()}";
		}
	}
}