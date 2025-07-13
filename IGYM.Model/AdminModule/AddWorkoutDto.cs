using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.AdminModule
{
	public class AddWorkoutDto
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		public string Description { get; set; }

		[Required]
		[StringLength(50)]
		public string Category { get; set; }

		[Required]
		[Range(1, 240)]
		public int DurationMinutes { get; set; }

		[StringLength(20)]
		public string Difficulty { get; set; }

		public string EquipmentNeeded { get; set; }

		public IFormFile? ImageFile { get; set; } // Changed from byte[] to IFormFile
	}
}
