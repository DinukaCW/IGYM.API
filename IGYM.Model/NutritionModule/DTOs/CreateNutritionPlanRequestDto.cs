using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.DTOs
{
	public class CreateNutritionPlanRequestDto
	{
		public int MemberId { get; set; }
		public int TrainerId { get; set; }
		public string Goal { get; set; }
		public float Weight { get; set; }
		public float Height { get; set; }
		public string DietPreference { get; set; }
		public string MedicalNotes { get; set; }
		public string AdditionalNotes { get; set; }
	}
}
