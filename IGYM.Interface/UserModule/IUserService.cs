using IGYM.Model.UserModule.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface.UserModule
{
	public interface IUserService
	{
		Task<LoginResult> LoginAsync(RequestLogin loginRequest);
		Task<LoginResult> LoginWithGoogleAsync(string idToken);
		Task<LoginResult> PasswordResetAsync(RequestLogin loginReq);
		Task<LoginResult> StoreNewpasswordAsync(PasswordReset passwordreset);
		Task<bool> LockAccountAsync(RequestLogin loginRequest);
		Task<bool> UnlockAccountAsync(RequestLogin loginRequest);
		Task<ServiceResult> CreateUserAsync(UserDetails userDetails);
		Task<ServiceResult> UpdateUserAsync(int userId, UserDetails userDetails);
		Task<UserDetails> GetUserDetailsByIdAsync(int userId);
		Task<List<UserReturn>> GetListOfUsersAsync();
	}
}
