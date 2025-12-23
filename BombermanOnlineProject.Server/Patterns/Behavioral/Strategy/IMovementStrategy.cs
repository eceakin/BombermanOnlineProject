using BombermanOnlineProject.Server.Core.Map;

namespace BombermanOnlineProject.Server.Patterns.Behavioral.Strategy
{
	public interface IMovementStrategy
	{
		(int X, int Y) GetNextMove(int currentX, int currentY, GameMap map, int? targetX = null, int? targetY = null);
		string GetStrategyName();
	}
}