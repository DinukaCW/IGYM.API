using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.UserModule.DTOs
{
	public class UserReturn 
	{
		public int UserId { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Name { get; set; }
		public int UserRoleId { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
	}
}
