using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class CreateSheduleRequest
	{
		public int MemberId { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public List<int> WorkoutIds { get; set; } = new List<int>();
		public int TrainerId { get; set; }
		public string Notes { get; set; }
	}
}
