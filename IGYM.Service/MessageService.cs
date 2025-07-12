using IGYM.Interface;
using IGYM.Interface.Data;
using IGYM.Model.UserModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Service
{
	public class MessageService : IMessageService
	{
		private readonly ILoginData _userData;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageService"/> class.
		/// </summary>

		public MessageService(ILoginData userData)
		{
			_userData = userData;
		}

		/// <summary>
		/// Generates a Multi-Factor Authentication (MFA) message for the specified user using the provided MFA code.
		/// </summary>
		/// <param name="user">The user for whom the MFA message is generated.</param>
		/// <param name="mfaCode">The MFA code to include in the message.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the generated message.</returns>
		public async Task<string> GenerateMfaMessage(User user, string mfaCode)
		{
			var message = await _userData.GetMessageAsync("MFAmessage");
			if (message != null)
			{
				var messageBody = message.MessageBody
					.Replace("{user.Username}", $"{user.Name ?? "Unknown"}")
					.Replace("{mfaCode}", mfaCode);

				return messageBody;
			}
			return "Message template not found";
		}


		/// <summary>
		/// Generates a locked account message for the specified user.
		/// </summary>
		/// <param name="user">The user for whom the locked message is generated.</param>
		/// <returns>A task that represents the asynchronous operation. The task result contains the generated message.</returns>
		public async Task<string> GenerateLockedMessage(User user)
		{
			// Retrieve the account locked message template from the database
			var message = await _userData.GetMessageAsync("Lockmessage");
			if (message != null)
			{
				// Replace placeholders with actual values
				var messageBody = message.MessageBody
					.Replace("{user.Username}", $"{user.Name ?? "Unknown"} {user.Name ?? "Unknown"}"); // Replace username placeholder

				return messageBody; // Return the generated message body
			}
			return "Message template not found";

		}

		/*public async Task<string> GenerateSMSMessage(User user, string mfaCode)
		{
			// Retrieve the MFA message template from the database
			var message = await _userData.GetMessageAsync("SMSmessage");
			if (message != null)
			{
				// Replace placeholders with actual values
				var messageBody = message.MessageBody
					.Replace("{user.Username}", $"{user.FirstName ?? "Unknown"} {user.LastName ?? "Unknown"}")  // Replace username placeholder
					.Replace("{mfaCode}", mfaCode); // Replace MFA code placeholder

				return messageBody; // Return the generated message body
			}
			return "Message template not found";

		}*/
	}
}
