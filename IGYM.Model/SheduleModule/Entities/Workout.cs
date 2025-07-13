using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.Entities
{
	public class Workout
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int WorkoutId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		public string Description { get; set; }

		[Required]
		[StringLength(50)]
		public string Category { get; set; } // Strength, Cardio, Flexibility, etc.

		[Required]
		public int DurationMinutes { get; set; }

		[StringLength(20)]
		public string Difficulty { get; set; }

		public string EquipmentNeeded { get; set; }

		public byte[]? Image { get; set; } // Optional image for the workout

		// Navigation properties
		public virtual ICollection<SheduleWorkout> ScheduledWorkouts { get; set; }
	}
}
