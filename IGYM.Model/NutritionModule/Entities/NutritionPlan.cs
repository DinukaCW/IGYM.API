using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.Entities
{
	public class NutritionPlan
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int RequestId { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public string? TrainerNotes { get; set; }

		// Navigation properties
		public virtual NutritionPlanRequest Request { get; set; }
		public virtual ICollection<MealPlan> MealPlans { get; set; }
	}
}
