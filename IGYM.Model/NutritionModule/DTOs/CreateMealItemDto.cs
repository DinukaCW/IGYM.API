using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.DTOs
{
	public class CreateMealItemDto
	{
		public int FoodItemId { get; set; }
		public float Quantity { get; set; }
		public string? PreparationNotes { get; set; }
	}
}
