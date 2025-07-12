using IGYM.Model.UserModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface.UserModule
{
	public interface IJwtTokenService
	{
		Task<string> GenerateJwtToken(User user);
		//Task<string> GenerateRefreshToken(User user);
		//Task<AuthenticatedResponse> RefreshToken(string refreshToken);
	}
}
