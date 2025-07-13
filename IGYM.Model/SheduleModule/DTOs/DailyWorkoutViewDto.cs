using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class DailyWorkoutViewDto
	{
		public int DayNumber { get; set; }
		public List<WorkoutDetailDto> Workouts { get; set; }
		public int TotalDurationMinutes { get; set; }
		public bool IsCompleted { get; set; }
	}
}
