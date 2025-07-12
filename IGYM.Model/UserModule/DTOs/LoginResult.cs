using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace IGYM.Model.UserModule.DTOs
{
	public class LoginResult
	{
		[Required]
		public bool Success { get; set; }
		public string? AccessToken { get; set; }
		public string? Message { get; set; }
		public int? UserId { get; set; }
		public bool UserLocked { get; set; } = false;
	}
}
