using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.DTOs
{
	public class NutritionPlanDto
	{
		public int Id { get; set; }
		public int RequestId { get; set; }
		public DateTime CreatedDate { get; set; }
		public string? TrainerNotes { get; set; }
		public string MemberName { get; set; }
		public string Goal { get; set; }
		public string DietPreference { get; set; }
		public List<MealPlanDto> MealPlans { get; set; }
	}
}
