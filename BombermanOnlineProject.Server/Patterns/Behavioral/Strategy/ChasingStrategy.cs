using BombermanOnlineProject.Server.Core.Map;

namespace BombermanOnlineProject.Server.Patterns.Behavioral.Strategy
{
	public class ChasingStrategy : IMovementStrategy
	{
		public (int X, int Y) GetNextMove(int currentX, int currentY, GameMap map, int? targetX = null, int? targetY = null)
		{
			if (!targetX.HasValue || !targetY.HasValue)
				return (currentX, currentY);

			var neighbors = map.GetWalkableNeighbors(currentX, currentY);

			if (neighbors.Count == 0)
				return (currentX, currentY);

			int minDistance = int.MaxValue;
			var bestMove = (currentX, currentY);

			foreach (var (nx, ny) in neighbors)
			{
				int distance = Math.Abs(nx - targetX.Value) + Math.Abs(ny - targetY.Value);

				if (distance < minDistance)
				{
					minDistance = distance;
					bestMove = (nx, ny);
				}
			}

			return bestMove;
		}

		public string GetStrategyName()
		{
			return "Chasing";
		}
	}
}