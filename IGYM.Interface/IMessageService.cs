using IGYM.Model.UserModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface
{
	public interface IMessageService
	{
		Task<string> GenerateMfaMessage(User user, string mfaCode);
		Task<string> GenerateLockedMessage(User user);
		//Task<string> GenerateSMSMessage(User user, string mfaCode);
	}
}
