using BombermanOnlineProject.Server.Core.Walls;

namespace BombermanOnlineProject.Server.Core.Walls
{
	/// <summary>
	/// Unbreakable wall that cannot be destroyed.
	/// Used for map boundaries and permanent obstacles.
	/// 
	/// Characteristics:
	/// - Blocks player movement
	/// - Stops explosion propagation
	/// - Cannot be destroyed
	/// - Forms the basic structure of the map
	/// </summary>
	public class UnbreakableWall : Wall
	{
		#region Constants

		private const char DEFAULT_DISPLAY_CHAR = '█';
		private const int INFINITE_HP = int.MaxValue;

		#endregion

		#region Constructor

		/// <summary>
		/// Creates an unbreakable wall at the specified position
		/// </summary>
		public UnbreakableWall(int x, int y)
			: base(x, y, isBreakable: false, hitPoints: INFINITE_HP, displayChar: DEFAULT_DISPLAY_CHAR)
		{
		}

		#endregion

		#region Overridden Methods

		/// <summary>
		/// Unbreakable walls cannot take damage.
		/// Always returns false (not destroyed).
		/// </summary>
		public override bool TakeDamage()
		{
			// Explosion stops here, wall is not affected
			return false;
		}

		/// <summary>
		/// Unbreakable walls cannot be destroyed
		/// </summary>
		public override void Destroy()
		{
			// This should never be called, but just in case:
			Console.WriteLine($"[WARNING] Attempted to destroy unbreakable wall at ({X}, {Y})");
		}

		#endregion

		#region Display Methods

		/// <summary>
		/// Gets the display character based on the theme
		/// </summary>
		public char GetThemedDisplayChar()
		{
			return Theme switch
			{
				WallTheme.Desert => '▓',   // Sand/stone pattern
				WallTheme.Forest => '♣',   // Tree symbol
				WallTheme.City => '■',     // Concrete block
				_ => DEFAULT_DISPLAY_CHAR
			};
		}

		#endregion
	}
}