using IGYM.Interface.Data;
using IGYM.Model;
using IGYM.Model.UserModule.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Data
{
	public class LoginData : ILoginData
	{
		private readonly IGYMDbContext _context;
		public LoginData(IGYMDbContext dbContext)
		{
			_context = dbContext;
		}

		public async Task<User> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
		{
			return await _context.Users
				.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
		}
		public async Task<User> GetUserByEmailAsync(string email)
		{
			return await _context.Users
				.FirstOrDefaultAsync(u => u.Email == email);
		}

		public async Task UpdateUserAsync(User user)
		{
			_context.Users.Update(user);
			await _context.SaveChangesAsync();
		}

		public async Task<List<User>> GetAllUsersAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<User> GetUserByIdAsync(int userId)
		{
			return await _context.Users.FindAsync(userId);
		}

		public async Task AddUserTokenAsync(UserToken userToken)
		{
			await _context.UserToken.AddAsync(userToken);
			await _context.SaveChangesAsync();
		}

		public async Task AddSendTokenAsync(SendToken sendToken)
		{
			await _context.SendToken.AddAsync(sendToken);
			await _context.SaveChangesAsync();
		}
		public async Task AddLoginTrackAsync(LoginTrack loginTrack)
		{
			await _context.LoginTrack.AddAsync(loginTrack);
			await _context.SaveChangesAsync();
		}

		public async Task<UserToken> GetUserTokenAsync(int userId, string hashedenteredMfa)
		{
			return await _context.UserToken
					.FirstOrDefaultAsync(ut => ut.UserID == userId && ut.Token == hashedenteredMfa && ut.TokenType == "MFA" &&
											   (ut.IsUsed == false || ut.IsUsed == null) && ut.ExpiresAt <= DateTime.Now);
		}

		public async Task UpdateUserTokenAsync(UserToken userToken)
		{
			_context.UserToken.Update(userToken);
			await _context.SaveChangesAsync();
		}
		public async Task UpdateLastLoginAsync(int userId)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user != null)
			{
				user.LastLoginAt = DateTime.Now;
				await _context.SaveChangesAsync();
			}
		}
		public async Task<UserToken> GetUserTokenAsync(string hashedRefToken)
		{
			return await _context.UserToken
					.FirstOrDefaultAsync(ut => ut.Token == hashedRefToken && ut.TokenType == "RefreshToken" && ut.IsUsed == false && ut.IsRevoked == false);
		}
		public async Task UpdateUserTokenUsedAsync(string hashedRefToken)
		{
			var storedRefreshToken = await _context.UserToken
					.FirstOrDefaultAsync(ut => ut.Token == hashedRefToken && ut.TokenType == "RefreshToken" && ut.IsUsed == false && ut.IsRevoked == false);
			if (storedRefreshToken != null)
			{
				storedRefreshToken.IsUsed = true;
				storedRefreshToken.LastUsedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
		}
		public async Task<Message> GetMessageAsync(string messageName)
		{
			return await _context.Message.FirstOrDefaultAsync(m => m.MessageName == messageName);

		}
		public async Task AddNotificatonLogAsync(SentNotification log)
		{
			await _context.SentNotification.AddAsync(log);
			await _context.SaveChangesAsync();
		}
		public async Task<User> CheckUserByUsernameOrEmailAsync(string userName, string email)
		{
			var normalizedUsername = userName.ToLower();
			var normalizedEmail = email.ToLower();

			return await _context.Users
				.FirstOrDefaultAsync(u => u.Username.ToLower() == normalizedUsername || u.Email.ToLower() == normalizedEmail);
		}
		public async Task CreateUserAsync(User user)
		{
			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
		}
		public async Task<User> CheckUserByUsernameOrEmailExceptIdAsync(int userId, string userName, string email)
		{
			return await _context.Users
				.Where(p => p.UserID != userId &&
						   (p.Username == userName || p.Email == email))
				.FirstOrDefaultAsync();
		}
		
		public async Task<string> GetUserRoleNameAsync(int userRoleId)
		{
			return await _context.UserRole
								 .Where(l => l.UserRoleID == userRoleId)
								 .Select(l => l.UserRoleName)
								 .FirstOrDefaultAsync();
		}
		public async Task AddUserHistoryAsync(UserHistory userHistory)
		{
			await _context.UserHistory.AddAsync(userHistory);
			await _context.SaveChangesAsync();
		}
	}
}
