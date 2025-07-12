using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class MemberSheduleSummaryDto
	{
		public int ScheduleId { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public string Status { get; set; }
		public int WorkoutCount { get; set; }
		public bool TrainerAssigned { get; set; }
	}
}
