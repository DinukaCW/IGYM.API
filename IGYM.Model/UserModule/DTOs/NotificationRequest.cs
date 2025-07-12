using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.UserModule.DTOs
{
	public class NotificationRequest
	{
		public List<string> Emails { get; set; } = new List<string>();

		[StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
		public string? Subject { get; set; }

		[StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
		public string? Message { get; set; }	
	}
}
