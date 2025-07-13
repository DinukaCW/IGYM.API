using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class SheduledWorkoutDto
	{
		public int WorkoutId { get; set; }
		public int SequenceOrder { get; set; }
		public int DurationMinutes { get; set; }
		public int RestMinutes { get; set; }
		public string Notes { get; set; }
	}
}
