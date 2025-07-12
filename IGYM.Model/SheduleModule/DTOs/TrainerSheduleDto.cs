using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class TrainerSheduleDto
	{
		public int ScheduleId { get; set; }
		public int ScheduledWorkoutId { get; set; }
		public int MemberId { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public string WorkoutName { get; set; }
		public string WorkoutCategory { get; set; }
		public int DurationMinutes { get; set; }
		public int SequenceOrder { get; set; }
		public string Status { get; set; }
		public bool Completed { get; set; }
		public DateTime? ActualStartTime { get; set; }
		public DateTime? ActualEndTime { get; set; }
		public string Notes { get; set; }
	}
}
