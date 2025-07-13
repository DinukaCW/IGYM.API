using IGYM.Model.SheduleModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class MemberWorkoutPlanDto
	{
		public int ScheduleId { get; set; }
		public string PlanName { get; set; }
		public string TrainerName { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public PlanStatus Status { get; set; }
		public string Notes { get; set; }
		public DateTime CreateDate { get; set; }
		public List<DailyWorkoutViewDto> DailyWorkouts { get; set; }
	}
}
