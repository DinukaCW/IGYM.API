using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.DTOs
{
	public class MealItemDto
	{
		public int FoodItemId { get; set; }
		public string FoodName { get; set; }
		public float Quantity { get; set; }
		public float Calories { get; set; }
		public float Protein { get; set; }
		public float Carbs { get; set; }
		public float Fats { get; set; }
		public string? PreparationNotes { get; set; }
	}

}
