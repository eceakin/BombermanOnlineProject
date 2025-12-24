using BombermanOnlineProject.Server.Data.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

namespace BombermanOnlineProject.Server.Data.Context
{

	public class BombermanDbContext : DbContext
	{
		public BombermanDbContext(DbContextOptions<BombermanDbContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; } = null!;
		public DbSet<GameStatistic> GameStatistics { get; set; } = null!;
		public DbSet<HighScore> HighScores { get; set; } = null!;
		public DbSet<PlayerPreference> PlayerPreferences { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>(entity =>
			{
				entity.HasKey(e => e.UserId);

				entity.HasIndex(e => e.Username)
					.IsUnique();

				entity.HasIndex(e => e.Email)
					.IsUnique();

				entity.HasIndex(e => e.CreatedAt);

				entity.Property(e => e.Username)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(e => e.Email)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.PasswordHash)
					.IsRequired()
					.HasMaxLength(255);

				entity.Property(e => e.CreatedAt)
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.IsActive)
					.HasDefaultValue(true);

				entity.Property(e => e.TotalGamesPlayed)
					.HasDefaultValue(0);

				entity.Property(e => e.TotalWins)
					.HasDefaultValue(0);

				entity.Property(e => e.TotalLosses)
					.HasDefaultValue(0);

				entity.Property(e => e.TotalKills)
					.HasDefaultValue(0);

				entity.Property(e => e.TotalDeaths)
					.HasDefaultValue(0);

				entity.Property(e => e.HighestScore)
					.HasDefaultValue(0);

				entity.HasMany(e => e.GameStatistics)
					.WithOne(e => e.User)
					.HasForeignKey(e => e.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasMany(e => e.HighScores)
					.WithOne(e => e.User)
					.HasForeignKey(e => e.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(e => e.PlayerPreference)
					.WithOne(e => e.User)
					.HasForeignKey<PlayerPreference>(e => e.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<GameStatistic>(entity =>
			{
				entity.HasKey(e => e.GameStatisticId);

				entity.HasIndex(e => e.UserId);
				entity.HasIndex(e => e.SessionId);
				entity.HasIndex(e => e.PlayedAt);
				entity.HasIndex(e => new { e.UserId, e.PlayedAt });

				entity.Property(e => e.SessionId)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.PlayedAt)
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.FinalScore)
					.HasDefaultValue(0);

				entity.Property(e => e.Kills)
					.HasDefaultValue(0);

				entity.Property(e => e.Deaths)
					.HasDefaultValue(0);

				entity.Property(e => e.BombsPlaced)
					.HasDefaultValue(0);

				entity.Property(e => e.WallsDestroyed)
					.HasDefaultValue(0);

				entity.Property(e => e.PowerUpsCollected)
					.HasDefaultValue(0);

				entity.Property(e => e.RoundsWon)
					.HasDefaultValue(0);

				entity.Property(e => e.RoundsLost)
					.HasDefaultValue(0);
			});

			modelBuilder.Entity<HighScore>(entity =>
			{
				entity.HasKey(e => e.HighScoreId);

				entity.HasIndex(e => e.UserId);
				entity.HasIndex(e => e.Score);
				entity.HasIndex(e => e.AchievedAt);
				entity.HasIndex(e => new { e.Score, e.AchievedAt });

				entity.Property(e => e.SessionId)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.Score)
					.IsRequired();

				entity.Property(e => e.AchievedAt)
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.Kills)
					.HasDefaultValue(0);

				entity.Property(e => e.Deaths)
					.HasDefaultValue(0);

				entity.Property(e => e.RoundsWon)
					.HasDefaultValue(0);
			});

			modelBuilder.Entity<PlayerPreference>(entity =>
			{
				entity.HasKey(e => e.PlayerPreferenceId);

				entity.HasIndex(e => e.UserId)
					.IsUnique();

				entity.Property(e => e.PreferredTheme)
					.HasMaxLength(20)
					.HasDefaultValue("Forest");

				entity.Property(e => e.PreferredDifficulty)
					.HasMaxLength(20)
					.HasDefaultValue("Normal");

				entity.Property(e => e.SoundEnabled)
					.HasDefaultValue(true);

				entity.Property(e => e.MusicEnabled)
					.HasDefaultValue(true);

				entity.Property(e => e.SoundVolume)
					.HasDefaultValue(70);

				entity.Property(e => e.MusicVolume)
					.HasDefaultValue(50);

				entity.Property(e => e.ShowGrid)
					.HasDefaultValue(true);

				entity.Property(e => e.ShowFPS)
					.HasDefaultValue(false);

				entity.Property(e => e.EnableParticles)
					.HasDefaultValue(true);

				entity.Property(e => e.AutoSaveReplays)
					.HasDefaultValue(false);

				entity.Property(e => e.KeyMoveUp)
					.HasMaxLength(10)
					.HasDefaultValue("W");

				entity.Property(e => e.KeyMoveDown)
					.HasMaxLength(10)
					.HasDefaultValue("S");

				entity.Property(e => e.KeyMoveLeft)
					.HasMaxLength(10)
					.HasDefaultValue("A");

				entity.Property(e => e.KeyMoveRight)
					.HasMaxLength(10)
					.HasDefaultValue("D");

				entity.Property(e => e.KeyPlaceBomb)
					.HasMaxLength(10)
					.HasDefaultValue("Space");

				entity.Property(e => e.CreatedAt)
					.HasDefaultValueSql("CURRENT_TIMESTAMP");

				entity.Property(e => e.UpdatedAt)
					.HasDefaultValueSql("CURRENT_TIMESTAMP");
			});

			SeedData(modelBuilder);
		}

		private void SeedData(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().HasData(
				new User
				{
					UserId = 1,
					Username = "admin",
					Email = "admin@bomberman.com",
					PasswordHash = "$2a$11$XK8PqKLqM5FZqGqVqKqFqOY9qKqFqOY9qKqFqOY9qKqFqOY9qK",
					CreatedAt = DateTime.UtcNow,
					IsActive = true,
					TotalGamesPlayed = 0,
					TotalWins = 0,
					TotalLosses = 0,
					PreferredTheme = "Forest"
				},
				new User
				{
					UserId = 2,
					Username = "player1",
					Email = "player1@bomberman.com",
					PasswordHash = "$2a$11$XK8PqKLqM5FZqGqVqKqFqOY9qKqFqOY9qKqFqOY9qKqFqOY9qK",
					CreatedAt = DateTime.UtcNow,
					IsActive = true,
					TotalGamesPlayed = 15,
					TotalWins = 10,
					TotalLosses = 5,
					TotalKills = 45,
					TotalDeaths = 20,
					HighestScore = 1250,
					PreferredTheme = "City"
				},
				new User
				{
					UserId = 3,
					Username = "player2",
					Email = "player2@bomberman.com",
					PasswordHash = "$2a$11$XK8PqKLqM5FZqGqVqKqFqOY9qKqFqOY9qKqFqOY9qKqFqOY9qK",
					CreatedAt = DateTime.UtcNow,
					IsActive = true,
					TotalGamesPlayed = 20,
					TotalWins = 8,
					TotalLosses = 12,
					TotalKills = 35,
					TotalDeaths = 40,
					HighestScore = 980,
					PreferredTheme = "Desert"
				}
			);

			modelBuilder.Entity<PlayerPreference>().HasData(
				new PlayerPreference
				{
					PlayerPreferenceId = 1,
					UserId = 1,
					PreferredTheme = "Forest",
					PreferredDifficulty = "Normal",
					SoundEnabled = true,
					MusicEnabled = true,
					SoundVolume = 70,
					MusicVolume = 50,
					ShowGrid = true,
					ShowFPS = false,
					EnableParticles = true,
					AutoSaveReplays = false,
					KeyMoveUp = "W",
					KeyMoveDown = "S",
					KeyMoveLeft = "A",
					KeyMoveRight = "D",
					KeyPlaceBomb = "Space",
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				},
				new PlayerPreference
				{
					PlayerPreferenceId = 2,
					UserId = 2,
					PreferredTheme = "City",
					PreferredDifficulty = "Hard",
					SoundEnabled = true,
					MusicEnabled = false,
					SoundVolume = 80,
					MusicVolume = 30,
					ShowGrid = false,
					ShowFPS = true,
					EnableParticles = true,
					AutoSaveReplays = true,
					KeyMoveUp = "Up",
					KeyMoveDown = "Down",
					KeyMoveLeft = "Left",
					KeyMoveRight = "Right",
					KeyPlaceBomb = "Space",
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				},
				new PlayerPreference
				{
					PlayerPreferenceId = 3,
					UserId = 3,
					PreferredTheme = "Desert",
					PreferredDifficulty = "Easy",
					SoundEnabled = true,
					MusicEnabled = true,
					SoundVolume = 60,
					MusicVolume = 60,
					ShowGrid = true,
					ShowFPS = false,
					EnableParticles = true,
					AutoSaveReplays = false,
					KeyMoveUp = "W",
					KeyMoveDown = "S",
					KeyMoveLeft = "A",
					KeyMoveRight = "D",
					KeyPlaceBomb = "E",
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				}
			);

			modelBuilder.Entity<HighScore>().HasData(
				new HighScore
				{
					HighScoreId = 1,
					UserId = 2,
					Score = 1250,
					AchievedAt = DateTime.UtcNow.AddDays(-5),
					SessionId = Guid.NewGuid().ToString(),
					Kills = 8,
					Deaths = 2,
					GameDuration = TimeSpan.FromMinutes(8.5),
					OpponentUsername = "player3",
					MapTheme = "City",
					RoundsWon = 3
				},
				new HighScore
				{
					HighScoreId = 2,
					UserId = 3,
					Score = 980,
					AchievedAt = DateTime.UtcNow.AddDays(-3),
					SessionId = Guid.NewGuid().ToString(),
					Kills = 6,
					Deaths = 3,
					GameDuration = TimeSpan.FromMinutes(7.2),
					OpponentUsername = "player1",
					MapTheme = "Desert",
					RoundsWon = 3
				},
				new HighScore
				{
					HighScoreId = 3,
					UserId = 2,
					Score = 1100,
					AchievedAt = DateTime.UtcNow.AddDays(-1),
					SessionId = Guid.NewGuid().ToString(),
					Kills = 7,
					Deaths = 1,
					GameDuration = TimeSpan.FromMinutes(9.0),
					OpponentUsername = "player2",
					MapTheme = "Forest",
					RoundsWon = 3
				}
			);
		}

		public override int SaveChanges()
		{
			UpdateTimestamps();
			return base.SaveChanges();
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			UpdateTimestamps();
			return base.SaveChangesAsync(cancellationToken);
		}

		private void UpdateTimestamps()
		{
			var entries = ChangeTracker.Entries()
				.Where(e => e.Entity is PlayerPreference && e.State == EntityState.Modified);

			foreach (var entry in entries)
			{
				if (entry.Entity is PlayerPreference preference)
				{
					preference.UpdatedAt = DateTime.UtcNow;
				}
			}
		}
	}
}
