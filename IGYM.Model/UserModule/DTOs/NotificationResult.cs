using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.UserModule.DTOs
{
	public class NotificationResult
	{
		public bool IsSuccess { get; set; }
		public List<string> ErrorMessages { get; set; } = new List<string>();
	}
}
