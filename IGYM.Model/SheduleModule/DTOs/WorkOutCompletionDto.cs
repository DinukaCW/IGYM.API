using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class WorkOutCompletionDto
	{
		public int ScheduledWorkoutId { get; set; }
		public bool Completed { get; set; }
		public string Notes { get; set; }
	}
}
