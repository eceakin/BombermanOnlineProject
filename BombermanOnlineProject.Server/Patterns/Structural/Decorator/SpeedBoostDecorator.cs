using BombermanOnlineProject.Server.Configuration;

namespace BombermanOnlineProject.Server.Patterns.Structural.Decorator
{
	/// <summary>
	/// DECORATOR PATTERN - Concrete Decorator
	/// 
	/// Purpose: Enhances player speed when collecting speed power-up.
	/// 
	/// How it works:
	/// 1. Wraps an IPlayer (could be base Player or another decorator)
	/// 2. Increases speed by multiplier from GameSettings
	/// 3. Respects maximum speed limit
	/// 4. All other functionality delegates to wrapped player
	/// 
	/// Usage Example:
	///   IPlayer player = new Player("Alice", 0, 1, 1);
	///   IPlayer boostedPlayer = new SpeedBoostDecorator(player);
	///   // Now boostedPlayer has increased speed
	/// 
	/// Stacking Example:
	///   IPlayer player = new Player("Bob", 1, 13, 11);
	///   player = new SpeedBoostDecorator(player);      // First boost
	///   player = new SpeedBoostDecorator(player);      // Second boost
	///   // Speed increases stack (with max limit)
	/// </summary>
	public class SpeedBoostDecorator : PlayerDecorator
	{
		#region Private Fields

		/// <summary>
		/// Speed boost amount applied by this decorator
		/// </summary>
		private readonly float _speedBoost;

		/// <summary>
		/// Original speed before this decorator was applied
		/// </summary>
		private readonly float _originalSpeed;

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a speed boost decorator for the player
		/// </summary>
		/// <param name="player">Player or decorator to wrap</param>
		/// <param name="customBoost">Optional custom boost amount (uses GameSettings if not provided)</param>
		public SpeedBoostDecorator(IPlayer player, float? customBoost = null)
			: base(player)
		{
			_originalSpeed = player.Speed;
			_speedBoost = customBoost ?? GameSettings.SpeedBoostMultiplier;

			// Calculate new speed with maximum limit
			float newSpeed = _originalSpeed + _speedBoost;
			player.Speed = Math.Min(newSpeed, GameSettings.MaxPlayerSpeed);

			Console.WriteLine($"[SpeedBoostDecorator] {player.PlayerName} speed: {_originalSpeed:F1} -> {player.Speed:F1}");
		}

		#endregion

		#region Overridden Properties

		/// <summary>
		/// Override Speed getter to ensure it returns enhanced value
		/// </summary>
		public override float Speed
		{
			get => _wrappedPlayer.Speed;
			set
			{
				// When speed is set, apply the boost
				float boostedSpeed = Math.Min(value + _speedBoost, GameSettings.MaxPlayerSpeed);
				_wrappedPlayer.Speed = boostedSpeed;
			}
		}

		#endregion

		#region Additional Methods

		/// <summary>
		/// Gets the speed boost amount provided by this decorator
		/// </summary>
		public float GetBoostAmount()
		{
			return _speedBoost;
		}

		/// <summary>
		/// Gets the original speed before this decorator was applied
		/// </summary>
		public float GetOriginalSpeed()
		{
			return _originalSpeed;
		}

		/// <summary>
		/// Checks if speed has reached maximum
		/// </summary>
		public bool IsAtMaxSpeed()
		{
			return _wrappedPlayer.Speed >= GameSettings.MaxPlayerSpeed;
		}

		#endregion
	}
}