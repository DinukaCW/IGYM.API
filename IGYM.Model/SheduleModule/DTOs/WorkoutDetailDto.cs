using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class WorkoutDetailDto
	{
		public int ScheduledWorkoutId { get; set; }
		public int WorkoutId { get; set; }
		public string WorkoutName { get; set; }
		public string Description { get; set; }
		public string Category { get; set; }
		public string Difficulty { get; set; }
		public string EquipmentNeeded { get; set; }
		public byte[]? Image { get; set; }
		public int SequenceOrder { get; set; }
		public int DurationMinutes { get; set; }
		public int RestMinutes { get; set; }
		public bool Completed { get; set; }
		public string Notes { get; set; }
	}
}
