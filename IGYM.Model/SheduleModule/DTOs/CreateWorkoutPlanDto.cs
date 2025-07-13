using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class CreateWorkoutPlanDto
	{
		public int MembersheduleRequestId { get; set; }
		public string PlanName { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Notes { get; set; }
		public List<DailyWorkOutPlanDto> DailyWorkouts { get; set; }
	}
}
