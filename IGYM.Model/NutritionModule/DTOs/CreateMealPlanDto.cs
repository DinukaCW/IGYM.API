using IGYM.Model.NutritionModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.DTOs
{
	public class CreateMealPlanDto
	{
		public MealType MealType { get; set; }
		public List<CreateMealItemDto> MealItems { get; set; }
	}
}
