using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.UserModule.Entities
{
	public class UserRole
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserRoleID { get; set; }
		public string UserRoleName { get; set; }
		public string Description { get; set; }
		public bool IsActive { get; set; }
	}
}
