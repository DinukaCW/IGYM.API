using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class MemberSheduleDetailDto
	{
		public int ScheduleId { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public string Status { get; set; }
		public string Notes { get; set; }
		public List<SheduleWorkoutDto> Workouts { get; set; } = new List<SheduleWorkoutDto>();
	}
}
