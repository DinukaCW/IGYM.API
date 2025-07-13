using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.AdminModule
{
	public class AddTrainerDto
	{
		[Required]
		[StringLength(100)]
		public string Username { get; set; }

		[Required]
		[StringLength(255)]
		public string Name { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }

		[StringLength(15)]
		public string? PhoneNumber { get; set; }

		[StringLength(100)]
		public string Specialization { get; set; }

		[Column(TypeName = "decimal(10,2)")]
		public decimal HourlyRate { get; set; }

		[StringLength(100)]
		public string AvailableDays { get; set; }

		[StringLength(100)]
		public string WorkingHours { get; set; }
	}
}
