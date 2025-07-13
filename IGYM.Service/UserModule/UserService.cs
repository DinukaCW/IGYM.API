using Google.Apis.Auth;
using IGYM.Interface;
using IGYM.Interface.Data;
using IGYM.Interface.UserModule;
using IGYM.Model;
using IGYM.Model.UserModule.DTOs;
using IGYM.Model.UserModule.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Service.UserModule
{
	public class UserService : IUserService
	{
		private readonly IEncryptionService _encryption;
		private readonly ILoginData _userData;
		private readonly ILogger<UserService> _logger;
		private readonly TrackLogin _trackLogin;
		private readonly IMfaService _mfaService;
		private readonly IJwtTokenService _jwtTokenService;
		private readonly IGYMDbContext _dbContext;
		private readonly IConfiguration _configuration;
		public UserService(IEncryptionService encryption, IConfiguration configuration, ILoginData loginData, ILogger<UserService> logger, TrackLogin trackLogin, IMfaService mfaService, IJwtTokenService jwtTokenService, IGYMDbContext dbContext)
		{
			_encryption = encryption;
			_userData = loginData;
			_logger = logger;
			_trackLogin = trackLogin;
			_mfaService = mfaService;
			_dbContext = dbContext;
			_jwtTokenService = jwtTokenService;
			_configuration = configuration;

		}

		/// <summary>
		/// Handles user login attempts, including password verification, account lockout for multiple failed attempts, 
		/// multi-factor authentication (MFA) checks, and JWT token generation upon successful login.
		/// </summary>
		/// <param name="loginRequest">Request object containing login details such as username or email and password.</param>
		/// <returns>LoginResult object indicating success, whether MFA is required, and any error messages.</returns>
		public async Task<LoginResult> LoginAsync(RequestLogin loginRequest)
		{
			try
			{
				_logger.LogInformation("Login attempt started for the user with username or email: {UsernameOrEmail}.", loginRequest.UsernameOrEmail);

				// Fetch user by username or email
				var user = await _userData.GetUserByUsernameOrEmailAsync(_encryption.EncryptData(loginRequest.UsernameOrEmail));

				if (user != null)
				{
					user.RememberMe = loginRequest.RememberMe;
				}

				// If the user doesn't exist or the password is incorrect
				if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
				{
					// Increment failed login count and lock account if necessary
					if (user != null)
					{
						user.FailedLoginCount++;
						if (user.FailedLoginCount >= 4)
						{
							user.IsLocked = true;
							await _userData.UpdateUserAsync(user);
							_logger.LogWarning("Account locked due to multiple failed login attempts for user: {FullName}.", user.Name);
							return new LoginResult { Success = false, UserLocked = true, Message = "Account locked due to multiple failed attempts. Please contact the administrator." };
						}
						await _userData.UpdateUserAsync(user);
						await _trackLogin.TrackLoginAsync(user, false, "Password", false, null, "Incorrect Password");
					}
					_logger.LogWarning("Invalid login attempt for user: {FullName}.", loginRequest.UsernameOrEmail);
					return new LoginResult { Success = false, Message = "Invalid username or password." };
				}

				// Check if the user's account is locked
				if (user.IsLocked)
				{
					await _trackLogin.TrackLoginAsync(user, false, "Password", false, null, "User Locked");
					_logger.LogWarning("Login attempt for a locked account: {FullName}.", user.Name);
					return new LoginResult { Success = false, UserLocked = true, Message = "Account is locked .Please Contact Admin" };
				}

				// If no MFA is required, reset failed login count and generate access tokens
				user.FailedLoginCount = 0;
				user.LastLoginAt = DateTime.Now;
				await _userData.UpdateUserAsync(user);
				_logger.LogInformation("Updated the last login time for user: {UserId}.", user.UserID);

				// Track login attempt
				await _trackLogin.TrackLoginAsync(user, true, "Password", false, null, null);

				//Genereate Tokens
				var Accesstoken = await _jwtTokenService.GenerateJwtToken(user);
				string userRole = await _userData.GetUserRoleNameAsync(user.UserRoleId);
				//var RefreshToken = await _jwtTokenService.GenerateRefreshToken(user);

				_logger.LogInformation("Login successful without MFA for user: {UserId}.", user.UserID);
				return new LoginResult { Success = true, AccessToken = Accesstoken,  Message = "Successfully Logged", UserId = user.UserID , UserRole = userRole};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred during login for the user with username or email: {UsernameOrEmail}.", loginRequest.UsernameOrEmail);
				return new LoginResult { Success = false, Message = "An error occurred during login. Please try again." };
			}
		}
		public async Task<LoginResult> LoginWithGoogleAsync(string idToken)
		{
			try
			{
				var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

				// Google ID token validated, extract user info
				var email = payload.Email;
				var name = payload.Name;
				var googleUserId = payload.Subject;

				// Check if the user exists
				var encryptedEmail = _encryption.EncryptData(email);
				var user = await _userData.GetUserByEmailAsync(encryptedEmail);

				if (user == null)
				{
					// Create a new user
					user = new User
					{						
						Username = encryptedEmail, // or use Google ID
						Name = name,
						PasswordHash = null, // No password for Google login
						Email = encryptedEmail,
						UserRoleId = 3, // Default to regular user role
						IsActive = true,
						IsEmailVerified = true,
						IsPhoneNumberVerified = false,
						CreatedAt = DateTime.UtcNow
					};

					await _userData.CreateUserAsync(user);
					_logger.LogInformation("New user created using Google account: {Email}", email);
				}

				// Generate JWT token
				var accessToken = await _jwtTokenService.GenerateJwtToken(user);

				await _trackLogin.TrackLoginAsync(user, true, "Google", false, null, null);
				return new LoginResult
				{
					Success = true,
					AccessToken = accessToken,
					Message = "Google login successful.",
					UserId = user.UserID
				};
			}
			catch (InvalidJwtException ex)
			{
				_logger.LogError(ex, "Invalid Google ID token.");
				return new LoginResult
				{
					Success = false,
					Message = "Invalid Google token."
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during Google login.");
				return new LoginResult
				{
					Success = false,
					Message = "Error during Google login. Please try again."
				};
			}
		}

		public async Task<LoginResult> PasswordResetAsync(RequestLogin loginReq)
		{
			try
			{
				// Attempt to find the user by email or username.
				var user = await _userData.GetUserByUsernameOrEmailAsync(loginReq.UsernameOrEmail);

				// If the user is not found, log a warning and return a failure result.
				if (user == null)
				{
					_logger.LogWarning("User with email or phone {UsernameOrEmail} not found for password reset", loginReq.UsernameOrEmail);
					return new LoginResult { Success = false, Message = "User not found" };
				}

				_logger.LogInformation("Initiates Password Reset for User {UsernameOrEmail}", loginReq.UsernameOrEmail);

				
				string message = string.Empty;
				string email = user.Email != null ? _encryption.DecryptData(user.Email) : null;

				// Send the MFA code and check if it was sent successfully.
				
				bool result = await _mfaService.SendMfaCodeAsync(user);
				if (result)
				{
					_logger.LogInformation("Password Reset code sent to user {UserId}.", user.UserID);
					return new LoginResult { Success = true, UserId = user.UserID, Message = message };
				}

				_logger.LogWarning("An error occurred while sending MFA code.");
				return new LoginResult { Success = false, Message = "Unable to send MFA Code" };

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred during MFA password reset for {UsernameOrEmail}.", loginReq.UsernameOrEmail);
				return new LoginResult { Success = false, Message = "An error occurred. Please try again." };
			}
		}

		public async Task<LoginResult> StoreNewpasswordAsync(PasswordReset passwordreset)
		{
			try
			{
				// Check if the new password matches the confirmed password.
				if (passwordreset.NewPassword != passwordreset.ConfirmPassword)
				{
					return new LoginResult { Success = false, Message = "New Password and Confirm Password do not match." };
				}

				// Attempt to find the user by their user ID.
				var user = await _userData.GetUserByIdAsync(passwordreset.UserId);
				if (user == null)
				{
					// Return a failure result if the user is not found.
					return new LoginResult { Success = false, Message = "User not found." };
				}

				// Hash the new password and update the user's password in the database.
				user.PasswordHash = _encryption.EncryptData(passwordreset.NewPassword);
				await _dbContext.SaveChangesAsync();

				_logger.LogInformation("Password reset successfully for user {UserId}.", user.UserID);
				return new LoginResult { Success = true, Message = "Password reset successfully." };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while resetting password for user {UserId}.", passwordreset.UserId);
				return new LoginResult { Success = false, Message = "An error occurred. Please try again." };
			}
		}
		public async Task<bool> LockAccountAsync(RequestLogin loginRequest)
		{
			try
			{
				// Retrieve the user by email or username
				var user = await _userData.GetUserByUsernameOrEmailAsync(loginRequest.UsernameOrEmail);
				// If the user is not found, log a warning and return false
				if (user == null)
				{
					_logger.LogWarning("User with email or phone {EmailOrPhoneNumber} not found.", loginRequest.UsernameOrEmail);
					return false;
				}

				// Lock the user's account
				user.IsActive = false;
				user.IsLocked = true;
				await _userData.UpdateUserAsync(user);
				_logger.LogInformation("Account locked for user {UserId}.", user.UserID);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while locking the account for {UsernameOrEmail}.", loginRequest.UsernameOrEmail);
				return false;
			}
		}

		public async Task<bool> UnlockAccountAsync(RequestLogin loginRequest)
		{
			try
			{
				// Retrieve the user by email or username
				var user = await _userData.GetUserByUsernameOrEmailAsync(loginRequest.UsernameOrEmail);
				// If the user is not found, log a warning and return false
				if (user == null)
				{
					_logger.LogWarning("User with email or phone {EmailOrPhoneNumber} not found.", loginRequest.UsernameOrEmail);
					return false;
				}
				// Unlock the user's account
				user.IsActive = true;
				user.IsLocked = false;
				await _userData.UpdateUserAsync(user);
				_logger.LogInformation("Account Unlocked for user {UserId}.", user.UserID);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while unlocking the account for {UsernameOrEmail}.", loginRequest.UsernameOrEmail);
				return false;
			}
		}
		/// <summary>
		/// Creates a new user account
		/// </summary>
		/// <param name="userDetails">User details for new account</param>
		/// <returns>Service result with success status</returns>
		public async Task<ServiceResult> CreateUserAsync(UserDetails userDetails)
		{
			try
			{
				_logger.LogInformation("Creating user: {Username}", userDetails.Username);

				// Check for existing user
				var encryptedUsername = _encryption.EncryptData(userDetails.Username);
				var encryptedEmail = _encryption.EncryptData(userDetails.Email);
				var existingUser = await _userData.CheckUserByUsernameOrEmailAsync(encryptedUsername, encryptedEmail);
				if (existingUser != null)
				{
					_logger.LogWarning("Duplicate user: {Username}", userDetails.Username);
					return new ServiceResult
					{
						Success = false,
						Message = "Username or Email already exists."
					};
				}

				// Validate password match
				if (userDetails.Password != userDetails.ReEnteredPassword)
				{
					_logger.LogWarning("Password mismatch for user: {Username}", userDetails.Username);
					return new ServiceResult
					{
						Success = false,
						Message = "Passwords do not match."
					};
				}

				// Create user entity
				var user = new User
				{
					Username = encryptedUsername,
					Name = userDetails.Name,
					PasswordHash = _encryption.EncryptData(userDetails.Password),
					UserRoleId = userDetails.UserRoleId,
					Email = encryptedEmail,
					PhoneNumber = _encryption.EncryptData(userDetails.PhoneNumber),
					IsEmailVerified = true,
					IsPhoneNumberVerified = true,
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				};

				await _userData.CreateUserAsync(user);

				_logger.LogInformation("User created: {UserId}", user.UserID);
				return new ServiceResult
				{
					Success = true,
					Message = "User created successfully.",
					UserId = user.UserID
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "User creation error: {Username}", userDetails.Username);
				return new ServiceResult
				{
					Success = false,
					Message = "User creation error. Please try again."
				};
			}
		}

		/// <summary>
		/// Updates an existing user account
		/// </summary>
		/// <param name="userId">ID of user to update</param>
		/// <param name="userDetails">Updated user details</param>
		/// <returns>Service result with success status</returns>
		public async Task<ServiceResult> UpdateUserAsync(int userId, UserDetails userDetails)
		{
			try
			{
				_logger.LogInformation("Updating user: {UserId}", userId);

				// Get existing user
				var exUser = await _userData.GetUserByIdAsync(userId);
				if (exUser == null)
				{
					_logger.LogWarning("User not found: {UserId}", userId);
					return new ServiceResult
					{
						Success = false,
						Message = "User not found."
					};
				}

				// Check for duplicate username/email if changed
				bool usernameChanged = !string.Equals(exUser.Username, _encryption.EncryptData(userDetails.Username),
					StringComparison.OrdinalIgnoreCase);
				bool emailChanged = !string.Equals(exUser.Email, _encryption.EncryptData(userDetails.Email),
					StringComparison.OrdinalIgnoreCase);

				if (usernameChanged || emailChanged)
				{
					var existingUser = await _userData.CheckUserByUsernameOrEmailExceptIdAsync(
						userId, _encryption.EncryptData(userDetails.Username), _encryption.EncryptData(userDetails.Email));
					if (existingUser != null)
					{
						_logger.LogWarning("Duplicate user details: {UserId}", userId);
						return new ServiceResult
						{
							Success = false,
							Message = "Username or Email already exists."
						};
					}
				}

				// Update password if provided
				if (!string.IsNullOrWhiteSpace(userDetails.Password))
				{
					if (userDetails.Password != userDetails.ReEnteredPassword)
					{
						_logger.LogWarning("Password mismatch for user: {UserId}", userId);
						return new ServiceResult
						{
							Success = false,
							Message = "Passwords do not match."
						};
					}
					exUser.PasswordHash = _encryption.EncryptData(userDetails.Password);
				}

				// Update user properties
				exUser.Username = _encryption.EncryptData(userDetails.Username);
				exUser.Name = userDetails.Name;
				exUser.UserRoleId = userDetails.UserRoleId;
				exUser.Email = _encryption.EncryptData(userDetails.Email);
				exUser.PhoneNumber = _encryption.EncryptData(userDetails.PhoneNumber);

				await _userData.UpdateUserAsync(exUser);

				_logger.LogInformation("User updated: {UserId}", userId);
				return new ServiceResult
				{
					Success = true,
					Message = "User updated successfully.",
					UserId = exUser.UserID
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "User update error: {UserId}", userId);
				return new ServiceResult
				{
					Success = false,
					Message = "User update error. Please try again."
				};
			}
		}

		/// <summary>
		/// Gets detailed information for a specific user
		/// </summary>
		/// <param name="userId">ID of user to retrieve</param>
		/// <returns>User details including personal info and settings</returns>
		public async Task<UserDetails> GetUserDetailsByIdAsync(int userId)
		{
			try
			{
				_logger.LogInformation("Getting details for user: {UserId}", userId);

				var user = await _userData.GetUserByIdAsync(userId);

				if (user == null)
				{
					_logger.LogWarning("User not found: {UserId}", userId);
					throw new InvalidOperationException("User Not found");
				}

				// Decrypt sensitive data
				var userDetails = new UserDetails
				{
					Username = _encryption.DecryptData(user.Username),
					Password = _encryption.DecryptData(user.PasswordHash),
					ReEnteredPassword = _encryption.DecryptData(user.PasswordHash),
					Name = user.Name,
					UserRoleId = user.UserRoleId,
					Email = _encryption.DecryptData(user.Email),
					PhoneNumber = _encryption.DecryptData(user.PhoneNumber)
				};

				_logger.LogInformation("Retrieved details for user: {UserId}", userId);
				return userDetails;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting details for user: {UserId}", userId);
				throw;
			}
		}

		/// <summary>
		/// Gets a list of all users in the system
		/// </summary>
		/// <returns>List of user information</returns>
		public async Task<List<UserReturn>> GetListOfUsersAsync()
		{
			try
			{
				_logger.LogInformation("Getting list of users");

				var users = await _userData.GetAllUsersAsync();
				var userDetailsList = new List<UserReturn>();

				foreach (var user in users)
				{
					userDetailsList.Add(new UserReturn
					{
						UserId = user.UserID,
						Username = _encryption.DecryptData(user.Username),
						Password = _encryption.DecryptData(user.PasswordHash),
						Name = user.Name,
						UserRoleId = user.UserRoleId,
						Email = _encryption.DecryptData(user.Email),
						PhoneNumber = _encryption.DecryptData(user.PhoneNumber),
					});
				}

				_logger.LogInformation("Retrieved {Count} users", userDetailsList.Count);
				return userDetailsList;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user list");
				throw;
			}
		}

		private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
		{
			try
			{
				string loginPassword = _encryption.EncryptData(enteredPassword);

				if (loginPassword != storedPasswordHash)
				{
					return false;
				}
				return true;
			}

			catch (Exception ex)
			{
				Console.WriteLine($"Error during password verification: {ex.Message}");
				return false;
			}
		}
	}
}
