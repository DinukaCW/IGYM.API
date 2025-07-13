using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.AdminModule
{
	public class WorkoutResponseDto
	{
		public int WorkoutId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Category { get; set; }
		public int DurationMinutes { get; set; }
		public string Difficulty { get; set; }
		public string EquipmentNeeded { get; set; }
		public string? Image { get; set; } // Base64 string
	}
}
