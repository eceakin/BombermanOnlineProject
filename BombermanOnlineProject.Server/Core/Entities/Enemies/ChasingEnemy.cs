using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Patterns.Behavioral.Strategy;

namespace BombermanOnlineProject.Server.Core.Entities.Enemies
{
	public class ChasingEnemy : Enemy
	{
		public int DetectionRange { get; private set; }

		public ChasingEnemy(int x, int y)
			: base(x, y,
				   name: "Chasing Enemy",
				   speed: GameSettings.ChasingEnemySpeed,
				   scoreValue: 100,
				   displayChar: 'C',
				   strategy: new ChasingStrategy())
		{
			DetectionRange = GameSettings.ChasingEnemyDetectionRange;
		}

		public bool IsPlayerInRange(int playerX, int playerY)
		{
			int distance = Math.Abs(X - playerX) + Math.Abs(Y - playerY);
			return distance <= DetectionRange;
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
		}
	}
}