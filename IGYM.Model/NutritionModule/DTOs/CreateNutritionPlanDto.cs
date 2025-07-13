using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.DTOs
{
	public class CreateNutritionPlanDto
	{
		public int RequestId { get; set; }
		public string? TrainerNotes { get; set; }
		public List<CreateMealPlanDto> MealPlans { get; set; }
	}
}
