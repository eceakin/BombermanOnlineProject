using BombermanOnlineProject.Server.Configuration;

namespace BombermanOnlineProject.Server.Patterns.Structural.Decorator
{
	/// <summary>
	/// DECORATOR PATTERN - Concrete Decorator
	/// 
	/// Purpose: Enhances maximum number of bombs player can place simultaneously.
	/// 
	/// How it works:
	/// 1. Wraps an IPlayer
	/// 2. Increases MaxBombs by increment from GameSettings
	/// 3. Respects maximum bomb count limit
	/// 4. Enables aggressive bombing strategies
	/// 
	/// Game Impact:
	/// - Base: 1 bomb at a time
	/// - With 1 power-up: 2 bombs
	/// - With 2 power-ups: 3 bombs
	/// - Maximum: 8 simultaneous bombs (from GameSettings)
	/// 
	/// Strategic Value:
	/// - More bombs = more control over map
	/// - Can create chain reactions
	/// - Can trap opponents more effectively
	/// - Requires skill to manage multiple bombs
	/// 
	/// Usage Example:
	///   IPlayer player = new Player("Bob", 1, 13, 11);
	///   player = new BombCountDecorator(player);
	///   // Player can now place 2 bombs simultaneously
	/// </summary>
	public class BombCountDecorator : PlayerDecorator
	{
		#region Private Fields

		/// <summary>
		/// Bomb count increment provided by this decorator
		/// </summary>
		private readonly int _countIncrement;

		/// <summary>
		/// Original max bombs before this decorator was applied
		/// </summary>
		private readonly int _originalMaxBombs;

		#endregion

		#region Constructor

		/// <summary>
		/// Creates a bomb count decorator for the player
		/// </summary>
		/// <param name="player">Player or decorator to wrap</param>
		/// <param name="customIncrement">Optional custom increment (uses GameSettings if not provided)</param>
		public BombCountDecorator(IPlayer player, int? customIncrement = null)
			: base(player)
		{
			_originalMaxBombs = player.MaxBombs;
			_countIncrement = customIncrement ?? GameSettings.BombCountIncrement;

			// Calculate new max bombs with limit
			int newMaxBombs = _originalMaxBombs + _countIncrement;
			player.MaxBombs = Math.Min(newMaxBombs, GameSettings.MaxBombCount);

			Console.WriteLine($"[BombCountDecorator] {player.PlayerName} max bombs: {_originalMaxBombs} -> {player.MaxBombs}");
		}

		#endregion

		#region Overridden Properties

		/// <summary>
		/// Override MaxBombs getter to ensure it returns enhanced value
		/// </summary>
		public override int MaxBombs
		{
			get => _wrappedPlayer.MaxBombs;
			set
			{
				// When max bombs is set, apply the increment
				int enhancedCount = Math.Min(value + _countIncrement, GameSettings.MaxBombCount);
				_wrappedPlayer.MaxBombs = enhancedCount;
			}
		}

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Override CanPlaceBomb to utilize enhanced bomb count
		/// </summary>
		public override bool CanPlaceBomb()
		{
			// Player can place bomb if active bombs < enhanced max bombs
			return _wrappedPlayer.ActiveBombs < _wrappedPlayer.MaxBombs && _wrappedPlayer.IsAlive;
		}

		#endregion

		#region Additional Methods

		/// <summary>
		/// Gets the bomb count increment provided by this decorator
		/// </summary>
		public int GetCountIncrement()
		{
			return _countIncrement;
		}

		/// <summary>
		/// Gets the original max bombs before this decorator was applied
		/// </summary>
		public int GetOriginalMaxBombs()
		{
			return _originalMaxBombs;
		}

		/// <summary>
		/// Checks if bomb count has reached maximum
		/// </summary>
		public bool IsAtMaxBombCount()
		{
			return _wrappedPlayer.MaxBombs >= GameSettings.MaxBombCount;
		}

		/// <summary>
		/// Gets available bomb slots (how many more bombs can be placed)
		/// </summary>
		public int GetAvailableBombSlots()
		{
			return Math.Max(0, _wrappedPlayer.MaxBombs - _wrappedPlayer.ActiveBombs);
		}

		/// <summary>
		/// Checks if player has any available bomb slots
		/// </summary>
		public bool HasAvailableSlots()
		{
			return GetAvailableBombSlots() > 0;
		}

		#endregion
	}
}