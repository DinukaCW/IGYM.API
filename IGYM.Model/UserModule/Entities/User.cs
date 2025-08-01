﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.UserModule.Entities
{
	public class User
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int UserID { get; set; }
		[Required]
		public string Username { get; set; }
		[Required]
		[StringLength(255)]
		public string Name { get; set; }
		public string? PasswordHash { get; set; }
		public int UserRoleId { get; set; }
		public string Email { get; set; }
		[StringLength(15)]
		public string? PhoneNumber { get; set; }
		public bool IsPhoneNumberVerified { get; set; } = false;
		public bool IsEmailVerified { get; set; } = false;
		public bool IsActive { get; set; } = true;
		public bool IsLocked { get; set; } = false;
		public bool RememberMe { get; set; } = false;
		public int FailedLoginCount { get; set; } = 0;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public DateTime? LastLoginAt { get; set; }

	}
}
