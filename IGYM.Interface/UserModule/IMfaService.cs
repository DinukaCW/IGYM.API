using IGYM.Model.UserModule.DTOs;
using IGYM.Model.UserModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface.UserModule
{
	public interface IMfaService
	{
		Task<LoginResult> ValidateMfaAsync(int userId, string enteredMfaCode);
		Task<bool> SendMfaCodeAsync(User user);
	}
}
