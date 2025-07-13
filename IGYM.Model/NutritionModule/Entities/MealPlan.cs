using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.Entities
{
	public class MealPlan
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int NutritionPlanId { get; set; }
		public MealType MealType { get; set; }

		// Navigation properties
		public virtual NutritionPlan NutritionPlan { get; set; }
		public virtual ICollection<MealItem> MealItems { get; set; }
	}
	public enum MealType
	{
		Breakfast,
		MorningSnack,
		Lunch,
		AfternoonSnack,
		Dinner,
		EveningSnack
	}
}
