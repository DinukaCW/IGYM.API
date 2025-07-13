using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
		public int DayNumber { get; set; } //  1st , 2nd , 3rd Date likewise
		public int WorkoutId { get; set; }

		[Required]
		public int SequenceOrder { get; set; } // To maintain workout order

		public int DurationMinutes { get; set; } // Duration of the workout in minutes
		public int RestMinutes { get; set; } // Rest time between workouts in minutes

		public bool Completed { get; set; } = false;

		public string Notes { get; set; }

		//Navigation properties
		public virtual MemberShedule MemberSchedule { get; set; }
		public virtual Workout Workout { get; set; }
	}
}
