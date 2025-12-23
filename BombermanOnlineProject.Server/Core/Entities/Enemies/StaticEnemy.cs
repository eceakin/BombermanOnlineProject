using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Patterns.Behavioral.Strategy;

namespace BombermanOnlineProject.Server.Core.Entities.Enemies
{
	public class StaticEnemy : Enemy
	{
		public StaticEnemy(int x, int y)
			: base(x, y,
				   name: "Static Enemy",
				   speed: GameSettings.DefaultEnemySpeed,
				   scoreValue: 50,
				   displayChar: 'S',
				   strategy: new RandomMovementStrategy())
		{
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);
		}
	}
}