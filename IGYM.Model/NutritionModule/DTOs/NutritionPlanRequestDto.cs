using IGYM.Model.NutritionModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.DTOs
{
	public class NutritionPlanRequestDto
	{
		public int Id { get; set; }
		public string MemberName { get; set; }
		public string Goal { get; set; }
		public float Weight { get; set; }
		public float Height { get; set; }
		public string DietPreference { get; set; }
		public DateTime RequestDate { get; set; }
		public NutritionPlanRequestStatus Status { get; set; }
	}
}
