using BombermanOnline.Server.Core.Walls;
using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Walls;

namespace BombermanOnlineProject.Client.Rendering
{
	/// <summary>
	/// Handles theme-specific rendering logic for the Bomberman game.
	/// 
	/// Responsibilities:
	/// - Theme-based character selection
	/// - Theme-based color schemes
	/// - Wall appearance customization per theme
	/// - Visual consistency across themes
	/// 
	/// Supported Themes:
	/// - Forest: Green, natural colors
	/// - Desert: Sand, yellow/brown tones
	/// - City: Gray, industrial colors
	/// 
	/// Design Principles:
	/// - Strategy Pattern: Different rendering strategies per theme
	/// - Open/Closed: Easy to add new themes without modifying existing code
	/// - Single Responsibility: Only handles theme-specific visual logic
	/// </summary>
	public class ThemeRenderer
	{
		#region Properties

		public GameSettings.GameTheme CurrentTheme { get; private set; }

		#endregion

		#region Theme Definitions

		// Theme-specific color palettes
		private static readonly Dictionary<GameSettings.GameTheme, ThemeColorScheme> _themeColors = new()
		{
			{
				GameSettings.GameTheme.Forest,
				new ThemeColorScheme
				{
					UnbreakableWall = ConsoleColor.DarkGreen,
					BreakableWall = ConsoleColor.Green,
					HardWall = ConsoleColor.DarkYellow,
					Ground = ConsoleColor.DarkGray,
					PlayerPanel = ConsoleColor.Cyan,
					InfoPanel = ConsoleColor.Yellow,
					Controls = ConsoleColor.Green,
					Accent = ConsoleColor.Green
				}
			},
			{
				GameSettings.GameTheme.Desert,
				new ThemeColorScheme
				{
					UnbreakableWall = ConsoleColor.DarkYellow,
					BreakableWall = ConsoleColor.Yellow,
					HardWall = ConsoleColor.DarkRed,
					Ground = ConsoleColor.DarkGray,
					PlayerPanel = ConsoleColor.Cyan,
					InfoPanel = ConsoleColor.Yellow,
					Controls = ConsoleColor.Yellow,
					Accent = ConsoleColor.Yellow
				}
			},
			{
				GameSettings.GameTheme.City,
				new ThemeColorScheme
				{
					UnbreakableWall = ConsoleColor.Gray,
					BreakableWall = ConsoleColor.DarkGray,
					HardWall = ConsoleColor.DarkCyan,
					Ground = ConsoleColor.Black,
					PlayerPanel = ConsoleColor.Cyan,
					InfoPanel = ConsoleColor.Yellow,
					Controls = ConsoleColor.White,
					Accent = ConsoleColor.Cyan
				}
			}
		};

		// Theme-specific wall characters
		private static readonly Dictionary<GameSettings.GameTheme, ThemeCharSet> _themeChars = new()
		{
			{
				GameSettings.GameTheme.Forest,
				new ThemeCharSet
				{
					UnbreakableWall = '♣',
					BreakableWall = '♠',
					HardWall = '♦',
					Ground = '·'
				}
			},
			{
				GameSettings.GameTheme.Desert,
				new ThemeCharSet
				{
					UnbreakableWall = '▓',
					BreakableWall = '░',
					HardWall = '▒',
					Ground = '·'
				}
			},
			{
				GameSettings.GameTheme.City,
				new ThemeCharSet
				{
					UnbreakableWall = '■',
					BreakableWall = '▒',
					HardWall = '▓',
					Ground = '·'
				}
			}
		};

		#endregion

		#region Constructor

		public ThemeRenderer(GameSettings.GameTheme theme = GameSettings.GameTheme.Forest)
		{
			CurrentTheme = theme;
		}

		#endregion

		#region Public Methods - Theme Management

		/// <summary>
		/// Changes the current theme
		/// </summary>
		public void SetTheme(GameSettings.GameTheme theme)
		{
			CurrentTheme = theme;
			Console.WriteLine($"[ThemeRenderer] Theme changed to: {theme}");
		}

		/// <summary>
		/// Changes theme by string name
		/// </summary>
		public bool SetThemeByName(string themeName)
		{
			if (Enum.TryParse<GameSettings.GameTheme>(themeName, true, out var theme))
			{
				SetTheme(theme);
				return true;
			}
			return false;
		}

		#endregion

		#region Public Methods - Color Retrieval

		/// <summary>
		/// Gets color for a specific wall type based on current theme
		/// </summary>
		public ConsoleColor GetWallColor(Wall wall, GameSettings.GameTheme? themeOverride = null)
		{
			var theme = themeOverride ?? CurrentTheme;

			if (!_themeColors.TryGetValue(theme, out var colorScheme))
			{
				return ConsoleColor.White; // Fallback
			}

			if (!wall.IsBreakable)
			{
				return colorScheme.UnbreakableWall;
			}
			else if (wall is HardWall)
			{
				return colorScheme.HardWall;
			}
			else
			{
				return colorScheme.BreakableWall;
			}
		}

		/// <summary>
		/// Gets theme color for UI elements
		/// </summary>
		public ConsoleColor GetThemeColor(GameSettings.GameTheme? themeOverride, string element)
		{
			var theme = themeOverride ?? CurrentTheme;

			if (!_themeColors.TryGetValue(theme, out var colorScheme))
			{
				return ConsoleColor.White;
			}

			return element.ToLower() switch
			{
				"playerpanel" => colorScheme.PlayerPanel,
				"infopanel" => colorScheme.InfoPanel,
				"controls" => colorScheme.Controls,
				"accent" => colorScheme.Accent,
				"ground" => colorScheme.Ground,
				_ => ConsoleColor.White
			};
		}

		/// <summary>
		/// Gets the complete color scheme for current theme
		/// </summary>
		public ThemeColorScheme GetColorScheme()
		{
			return _themeColors.TryGetValue(CurrentTheme, out var scheme)
				? scheme
				: _themeColors[GameSettings.GameTheme.Forest]; // Fallback
		}

		#endregion

		#region Public Methods - Character Retrieval

		/// <summary>
		/// Gets character for a specific wall type based on current theme
		/// </summary>
		public char GetWallChar(Wall wall, GameSettings.GameTheme? themeOverride = null)
		{
			var theme = themeOverride ?? CurrentTheme;

			if (!_themeChars.TryGetValue(theme, out var charSet))
			{
				return '█'; // Fallback
			}

			if (!wall.IsBreakable)
			{
				return charSet.UnbreakableWall;
			}
			else if (wall is HardWall)
			{
				return charSet.HardWall;
			}
			else
			{
				return charSet.BreakableWall;
			}
		}

		/// <summary>
		/// Gets ground character for current theme
		/// </summary>
		public char GetGroundChar()
		{
			return _themeChars.TryGetValue(CurrentTheme, out var charSet)
				? charSet.Ground
				: '·';
		}

		/// <summary>
		/// Gets the complete character set for current theme
		/// </summary>
		public ThemeCharSet GetCharSet()
		{
			return _themeChars.TryGetValue(CurrentTheme, out var charSet)
				? charSet
				: _themeChars[GameSettings.GameTheme.Forest]; // Fallback
		}

		#endregion

		#region Public Methods - Theme Information

		/// <summary>
		/// Gets all available themes
		/// </summary>
		public static IEnumerable<GameSettings.GameTheme> GetAvailableThemes()
		{
			return Enum.GetValues<GameSettings.GameTheme>();
		}

		/// <summary>
		/// Displays theme preview in console
		/// </summary>
		public void DisplayThemePreview()
		{
			Console.WriteLine($"\n═══ {CurrentTheme} Theme Preview ═══\n");

			var colorScheme = GetColorScheme();
			var charSet = GetCharSet();

			// Display wall samples
			Console.ForegroundColor = colorScheme.UnbreakableWall;
			Console.Write($"{charSet.UnbreakableWall}{charSet.UnbreakableWall} ");
			Console.ResetColor();
			Console.WriteLine("Unbreakable Wall");

			Console.ForegroundColor = colorScheme.BreakableWall;
			Console.Write($"{charSet.BreakableWall}{charSet.BreakableWall} ");
			Console.ResetColor();
			Console.WriteLine("Breakable Wall");

			Console.ForegroundColor = colorScheme.HardWall;
			Console.Write($"{charSet.HardWall}{charSet.HardWall} ");
			Console.ResetColor();
			Console.WriteLine("Hard Wall");

			Console.ForegroundColor = colorScheme.Ground;
			Console.Write($"{charSet.Ground}{charSet.Ground} ");
			Console.ResetColor();
			Console.WriteLine("Ground");

			Console.WriteLine();
		}

		#endregion

		#region Nested Classes - Theme Data Structures

		/// <summary>
		/// Defines color scheme for a theme
		/// </summary>
		public class ThemeColorScheme
		{
			public ConsoleColor UnbreakableWall { get; set; }
			public ConsoleColor BreakableWall { get; set; }
			public ConsoleColor HardWall { get; set; }
			public ConsoleColor Ground { get; set; }
			public ConsoleColor PlayerPanel { get; set; }
			public ConsoleColor InfoPanel { get; set; }
			public ConsoleColor Controls { get; set; }
			public ConsoleColor Accent { get; set; }
		}

		/// <summary>
		/// Defines character set for a theme
		/// </summary>
		public class ThemeCharSet
		{
			public char UnbreakableWall { get; set; }
			public char BreakableWall { get; set; }
			public char HardWall { get; set; }
			public char Ground { get; set; }
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Validates if a theme name is valid
		/// </summary>
		public static bool IsValidTheme(string themeName)
		{
			return Enum.TryParse<GameSettings.GameTheme>(themeName, true, out _);
		}

		/// <summary>
		/// Gets theme description
		/// </summary>
		public static string GetThemeDescription(GameSettings.GameTheme theme)
		{
			return theme switch
			{
				GameSettings.GameTheme.Forest => "Green forest environment with natural elements",
				GameSettings.GameTheme.Desert => "Sandy desert with warm earth tones",
				GameSettings.GameTheme.City => "Urban industrial setting with concrete structures",
				_ => "Unknown theme"
			};
		}

		#endregion
	}
}