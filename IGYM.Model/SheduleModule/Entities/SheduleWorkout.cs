using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.Entities
{
	public class SheduleWorkout
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ScheduledWorkoutId { get; set; }

		public int ScheduleId { get; set; }

		public int WorkoutId { get; set; }

		[Required]
		public int SequenceOrder { get; set; } // To maintain workout order
		public int? TrainerId { get; set; }

		public DateTime? ActualStartTime { get; set; }
		public DateTime? ActualEndTime { get; set; }

		public bool Completed { get; set; } = false;

		public string Notes { get; set; }
	}
}
