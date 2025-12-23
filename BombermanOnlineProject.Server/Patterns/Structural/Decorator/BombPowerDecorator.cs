using BombermanOnlineProject.Server.Configuration;

namespace BombermanOnlineProject.Server.Patterns.Structural.Decorator
{
	/// <summary>
	/// DECORATOR PATTERN - Concrete Decorator
	/// 
	/// Purpose: Enhances bomb explosion power when collecting bomb power-up.
	/// Increases the range of bomb explosions.
	/// 
	/// How it works:
	/// 1. Wraps an IPlayer
	/// 2. Increases BombPower by increment from GameSettings
	/// 3. Respects maximum bomb power limit
	/// 4. Creates more strategic gameplay (longer explosion range)
	/// 
	/// Game Impact:
	/// - Base power: 2 cells in each direction
	/// - With 1 power-up: 3 cells
	/// - With 2 power-ups: 4 cells
	/// - Maximum: 8 cells (from GameSettings)
	/// 
	/// Usage Example:
	///   IPlayer player = new Player("Alice", 0, 1, 1);
	///   player = new BombPowerDecorator(player);
	///   // Player's bombs now explode with +1 range
	/// </summary>
	public class BombPowerDecorator : PlayerDecorator
	{
		#region Private Fields

		/// <summary>
		/// Bomb power increment provided by this decorator
		/// </summary>
		private readonly int _powerIncrement;

		/// <summary>
		/// Original bomb power before this decorator was applied
		/// </summary>
		private readonly int _originalPower;

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a bomb power decorator for the player
		/// </summary>
		/// <param name="player">Player or decorator to wrap</param>
		/// <param name="customIncrement">Optional custom increment (uses GameSettings if not provided)</param>
		public BombPowerDecorator(IPlayer player, int? customIncrement = null)
			: base(player)
		{
			_originalPower = player.BombPower;
			_powerIncrement = customIncrement ?? GameSettings.BombPowerIncrement;

			// Calculate new power with maximum limit
			int newPower = _originalPower + _powerIncrement;
			player.BombPower = Math.Min(newPower, GameSettings.MaxBombPower);

			Console.WriteLine($"[BombPowerDecorator] {player.PlayerName} bomb power: {_originalPower} -> {player.BombPower}");
		}

		#endregion

		#region Overridden Properties

		/// <summary>
		/// Override BombPower getter to ensure it returns enhanced value
		/// </summary>
		public override int BombPower
		{
			get => _wrappedPlayer.BombPower;
			set
			{
				// When bomb power is set, apply the increment
				int enhancedPower = Math.Min(value + _powerIncrement, GameSettings.MaxBombPower);
				_wrappedPlayer.BombPower = enhancedPower;
			}
		}

		#endregion

		#region Additional Methods

		/// <summary>
		/// Gets the power increment provided by this decorator
		/// </summary>
		public int GetPowerIncrement()
		{
			return _powerIncrement;
		}

		/// <summary>
		/// Gets the original bomb power before this decorator was applied
		/// </summary>
		public int GetOriginalPower()
		{
			return _originalPower;
		}

		/// <summary>
		/// Checks if bomb power has reached maximum
		/// </summary>
		public bool IsAtMaxPower()
		{
			return _wrappedPlayer.BombPower >= GameSettings.MaxBombPower;
		}

		/// <summary>
		/// Calculates the explosion radius in cells
		/// </summary>
		public int GetExplosionRadius()
		{
			return _wrappedPlayer.BombPower;
		}

		#endregion
	}
}