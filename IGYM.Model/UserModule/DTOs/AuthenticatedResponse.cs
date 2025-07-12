using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.UserModule.DTOs
{
	public class AuthenticatedResponse
	{
		public string AccessToken { get; set; }  // JWT Access Token
		public DateTime Expiration { get; set; } // Expiry time of the access token
	}
}
