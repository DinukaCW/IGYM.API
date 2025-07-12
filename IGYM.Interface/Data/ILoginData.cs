using IGYM.Model;
using IGYM.Model.UserModule.Entities;

namespace IGYM.Interface.Data
{
	public interface ILoginData
	{
		Task<User> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
		Task<User> GetUserByEmailAsync(string email);
		Task UpdateUserAsync(User user);
		Task<List<User>> GetAllUsersAsync();
		Task<User> GetUserByIdAsync(int userId);
		Task AddUserTokenAsync(UserToken userToken);
		Task AddSendTokenAsync(SendToken sendToken);
		Task AddLoginTrackAsync(LoginTrack loginTrack);
		Task<UserToken> GetUserTokenAsync(int userId, string hashedenteredMfa);
		Task<Message> GetMessageAsync(string messageType);
		Task UpdateUserTokenAsync(UserToken userToken);
		Task UpdateLastLoginAsync(int userId);
		Task<UserToken> GetUserTokenAsync(string hashedRefToken);
		Task UpdateUserTokenUsedAsync(string hashedRefToken);
		Task AddNotificatonLogAsync(SentNotification log);
		Task<User> CheckUserByUsernameOrEmailAsync(string userName, string email);
		Task CreateUserAsync(User user);
		Task<User> CheckUserByUsernameOrEmailExceptIdAsync(int userId, string userName, string email);
		Task<string> GetUserRoleNameAsync(int userRoleId);
		Task AddUserHistoryAsync(UserHistory history);
	}
}
