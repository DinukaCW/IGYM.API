using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.Entities
{
	public class MealItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int MealPlanId { get; set; }
		public int FoodItemId { get; set; }
		public float Quantity { get; set; } // in grams or servings
		public string? PreparationNotes { get; set; }

		// Navigation properties
		public virtual MealPlan MealPlan { get; set; }
		public virtual FoodItem FoodItem { get; set; }
	}
}
