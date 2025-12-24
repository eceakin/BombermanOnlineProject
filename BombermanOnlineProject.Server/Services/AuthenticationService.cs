using BombermanOnlineProject.Server.Data.Models;
using BombermanOnlineProject.Server.Data.UnitOfWork;
using System.Security.Cryptography;

namespace BombermanOnlineProject.Server.Services
{
	public class AuthenticationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private const int SaltSize = 16;
		private const int HashSize = 32;
		private const int Iterations = 10000;

		public AuthenticationService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public async Task<User?> RegisterAsync(string username, string email, string password)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username cannot be empty", nameof(username));

			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Email cannot be empty", nameof(email));

			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentException("Password cannot be empty", nameof(password));

			if (await _unitOfWork.Users.UsernameExistsAsync(username))
			{
				throw new InvalidOperationException("Username already exists");
			}

			if (await _unitOfWork.Users.EmailExistsAsync(email))
			{
				throw new InvalidOperationException("Email already exists");
			}

			var passwordHash = HashPassword(password);

			var user = new User
			{
				Username = username,
				Email = email,
				PasswordHash = passwordHash,
				CreatedAt = DateTime.UtcNow,
				IsActive = true
			};

			await _unitOfWork.Users.AddAsync(user);
			await _unitOfWork.SaveChangesAsync();

			var preference = new PlayerPreference
			{
				UserId = user.UserId
			};

			await _unitOfWork.PlayerPreferences.AddAsync(preference);
			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[AuthenticationService] User registered: {username}");
			return user;
		}

		public async Task<User?> LoginAsync(string usernameOrEmail, string password)
		{
			if (string.IsNullOrWhiteSpace(usernameOrEmail))
				throw new ArgumentException("Username or email cannot be empty", nameof(usernameOrEmail));

			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentException("Password cannot be empty", nameof(password));

			var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(usernameOrEmail);

			if (user == null)
			{
				Console.WriteLine($"[AuthenticationService] Login failed: User not found");
				return null;
			}

			if (!user.IsActive)
			{
				Console.WriteLine($"[AuthenticationService] Login failed: User is inactive");
				return null;
			}

			if (!VerifyPassword(password, user.PasswordHash))
			{
				Console.WriteLine($"[AuthenticationService] Login failed: Invalid password");
				return null;
			}

			await _unitOfWork.Users.UpdateLastLoginAsync(user.UserId);
			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[AuthenticationService] User logged in: {user.Username}");
			return user;
		}

		public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(oldPassword))
				throw new ArgumentException("Old password cannot be empty", nameof(oldPassword));

			if (string.IsNullOrWhiteSpace(newPassword))
				throw new ArgumentException("New password cannot be empty", nameof(newPassword));

			var user = await _unitOfWork.Users.GetByIdAsync(userId);

			if (user == null)
			{
				Console.WriteLine($"[AuthenticationService] Change password failed: User not found");
				return false;
			}

			if (!VerifyPassword(oldPassword, user.PasswordHash))
			{
				Console.WriteLine($"[AuthenticationService] Change password failed: Invalid old password");
				return false;
			}

			user.PasswordHash = HashPassword(newPassword);
			await _unitOfWork.Users.UpdateAsync(user);
			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[AuthenticationService] Password changed for user: {user.Username}");
			return true;
		}

		public async Task<bool> ResetPasswordAsync(string email, string newPassword)
		{
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Email cannot be empty", nameof(email));

			if (string.IsNullOrWhiteSpace(newPassword))
				throw new ArgumentException("New password cannot be empty", nameof(newPassword));

			var user = await _unitOfWork.Users.GetByEmailAsync(email);

			if (user == null)
			{
				Console.WriteLine($"[AuthenticationService] Reset password failed: User not found");
				return false;
			}

			user.PasswordHash = HashPassword(newPassword);
			await _unitOfWork.Users.UpdateAsync(user);
			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[AuthenticationService] Password reset for user: {user.Username}");
			return true;
		}

		public async Task<User?> GetUserByIdAsync(int userId)
		{
			return await _unitOfWork.Users.GetByIdAsync(userId);
		}

		public async Task<User?> GetUserByUsernameAsync(string username)
		{
			return await _unitOfWork.Users.GetByUsernameAsync(username);
		}

		public async Task<bool> DeactivateAccountAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);

			if (user == null)
			{
				return false;
			}

			await _unitOfWork.Users.DeactivateUserAsync(userId);
			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[AuthenticationService] Account deactivated: {user.Username}");
			return true;
		}

		public async Task<bool> ActivateAccountAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);

			if (user == null)
			{
				return false;
			}

			await _unitOfWork.Users.ActivateUserAsync(userId);
			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[AuthenticationService] Account activated: {user.Username}");
			return true;
		}

		private string HashPassword(string password)
		{
			using var rng = RandomNumberGenerator.Create();
			var salt = new byte[SaltSize];
			rng.GetBytes(salt);

			using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
			var hash = pbkdf2.GetBytes(HashSize);

			var hashBytes = new byte[SaltSize + HashSize];
			Array.Copy(salt, 0, hashBytes, 0, SaltSize);
			Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

			return Convert.ToBase64String(hashBytes);
		}

		private bool VerifyPassword(string password, string hashedPassword)
		{
			try
			{
				var hashBytes = Convert.FromBase64String(hashedPassword);

				if (hashBytes.Length != SaltSize + HashSize)
				{
					return false;
				}

				var salt = new byte[SaltSize];
				Array.Copy(hashBytes, 0, salt, 0, SaltSize);

				using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
				var hash = pbkdf2.GetBytes(HashSize);

				for (int i = 0; i < HashSize; i++)
				{
					if (hashBytes[i + SaltSize] != hash[i])
					{
						return false;
					}
				}

				return true;
			}
			catch
			{
				return false;
			}
		}
	}


}
