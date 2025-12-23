using BombermanOnlineProject.Server.Core.Map;

namespace BombermanOnlineProject.Server.Patterns.Behavioral.Strategy
{
	public class RandomMovementStrategy : IMovementStrategy
	{
		private readonly Random _random;

		public RandomMovementStrategy()
		{
			_random = new Random();
		}

		public (int X, int Y) GetNextMove(int currentX, int currentY, GameMap map, int? targetX = null, int? targetY = null)
		{
			var neighbors = map.GetWalkableNeighbors(currentX, currentY);

			if (neighbors.Count == 0)
				return (currentX, currentY);

			var randomIndex = _random.Next(neighbors.Count);
			return neighbors[randomIndex];
		}

		public string GetStrategyName()
		{
			return "Random";
		}
	}
}