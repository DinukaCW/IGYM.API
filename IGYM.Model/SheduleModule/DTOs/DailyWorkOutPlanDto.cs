using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class DailyWorkOutPlanDto
	{
		public int DayNumber { get; set; }
		public List<SheduledWorkoutDto> Workouts { get; set; }
	}
}
