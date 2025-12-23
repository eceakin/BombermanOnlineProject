namespace BombermanOnlineProject.Server.Patterns.Structural.Decorator
{
	/// <summary>
	/// DECORATOR PATTERN - Abstract Decorator Base Class
	/// 
	/// Purpose: Serves as the base for all concrete decorators (power-ups).
	/// Implements IPlayer and contains a reference to another IPlayer.
	/// 
	/// How it works:
	/// 1. Holds a reference to the wrapped player (could be base Player or another decorator)
	/// 2. Delegates all operations to the wrapped player by default
	/// 3. Concrete decorators override specific methods to add behavior
	/// 
	/// Example decoration chain:
	/// BombPowerDecorator -> SpeedBoostDecorator -> Player
	/// 
	/// Benefits:
	/// - Transparent wrapping - decorators look like players
	/// - Stackable - multiple decorators can wrap each other
	/// - Runtime composition - power-ups applied dynamically
	/// - Single Responsibility - each decorator handles one enhancement
	/// </summary>
	public abstract class PlayerDecorator : IPlayer
	{
		#region Protected Fields

		/// <summary>
		/// The wrapped player instance (either base Player or another decorator)
		/// Protected so concrete decorators can access it
		/// </summary>
		protected readonly IPlayer _wrappedPlayer;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes the decorator with a player to wrap
		/// </summary>
		/// <param name="player">Player or decorator to wrap</param>
		protected PlayerDecorator(IPlayer player)
		{
			_wrappedPlayer = player ?? throw new ArgumentNullException(nameof(player));
		}

		#endregion

		#region IPlayer Implementation - Delegation

		// All properties and methods delegate to the wrapped player by default.
		// Concrete decorators override only what they need to enhance.

		public virtual string Id => _wrappedPlayer.Id;
		public virtual string PlayerName => _wrappedPlayer.PlayerName;
		public virtual int PlayerNumber => _wrappedPlayer.PlayerNumber;

		public virtual int X
		{
			get => _wrappedPlayer.X;
			set => _wrappedPlayer.X = value;
		}

		public virtual int Y
		{
			get => _wrappedPlayer.Y;
			set => _wrappedPlayer.Y = value;
		}

		public virtual bool IsAlive
		{
			get => _wrappedPlayer.IsAlive;
			set => _wrappedPlayer.IsAlive = value;
		}

		// These are the properties that decorators will typically override:
		public virtual float Speed
		{
			get => _wrappedPlayer.Speed;
			set => _wrappedPlayer.Speed = value;
		}

		public virtual int BombPower
		{
			get => _wrappedPlayer.BombPower;
			set => _wrappedPlayer.BombPower = value;
		}

		public virtual int MaxBombs
		{
			get => _wrappedPlayer.MaxBombs;
			set => _wrappedPlayer.MaxBombs = value;
		}

		public virtual int ActiveBombs
		{
			get => _wrappedPlayer.ActiveBombs;
			set => _wrappedPlayer.ActiveBombs = value;
		}

		public virtual char DisplayChar => _wrappedPlayer.DisplayChar;

		public virtual int Score
		{
			get => _wrappedPlayer.Score;
			set => _wrappedPlayer.Score = value;
		}

		public virtual bool IsInvulnerable => _wrappedPlayer.IsInvulnerable;

		public virtual int Kills
		{
			get => _wrappedPlayer.Kills;
			set => _wrappedPlayer.Kills = value;
		}

		public virtual int Deaths
		{
			get => _wrappedPlayer.Deaths;
			set => _wrappedPlayer.Deaths = value;
		}

		#endregion

		#region IPlayer Methods - Delegation

		public virtual void Update(float deltaTime)
		{
			_wrappedPlayer.Update(deltaTime);
		}

		public virtual bool CanPlaceBomb()
		{
			return _wrappedPlayer.CanPlaceBomb();
		}

		public virtual void PlaceBomb()
		{
			_wrappedPlayer.PlaceBomb();
		}

		public virtual void BombExploded()
		{
			_wrappedPlayer.BombExploded();
		}

		public virtual bool MoveTo(int newX, int newY)
		{
			return _wrappedPlayer.MoveTo(newX, newY);
		}

		public virtual void TakeDamage()
		{
			_wrappedPlayer.TakeDamage();
		}

		public virtual void Respawn(int spawnX, int spawnY)
		{
			_wrappedPlayer.Respawn(spawnX, spawnY);
		}

		public virtual void AddKill()
		{
			_wrappedPlayer.AddKill();
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Unwraps the decorator chain to get the base player
		/// Useful for debugging or accessing core player data
		/// </summary>
		public IPlayer GetBasePlayer()
		{
			IPlayer current = this;

			while (current is PlayerDecorator decorator)
			{
				current = decorator._wrappedPlayer;
			}

			return current;
		}

		/// <summary>
		/// Counts the number of decorators in the chain
		/// </summary>
		public int GetDecoratorDepth()
		{
			int depth = 0;
			IPlayer current = this;

			while (current is PlayerDecorator decorator)
			{
				depth++;
				current = decorator._wrappedPlayer;
			}

			return depth;
		}

		#endregion
	}
}