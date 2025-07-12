using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class SheduleWorkoutDto
	{
		public int ScheduledWorkoutId { get; set; }
		public string WorkoutName { get; set; }
		public string WorkoutDescription { get; set; }
		public string WorkoutCategory { get; set; }
		public int DurationMinutes { get; set; }
		public int SequenceOrder { get; set; }
		public string TrainerName { get; set; }
		public int? TrainerId { get; set; }
		public DateTime? ActualStartTime { get; set; }
		public DateTime? ActualEndTime { get; set; }
		public bool Completed { get; set; }
		public string Notes { get; set; }
	}
}
