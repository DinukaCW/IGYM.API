using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class TrainerDto
	{
		public int TrainerId { get; set; }
		public string Name { get; set; }
		public string Specialization { get; set; }
		public decimal HourlyRate { get; set; }
		public string AvailableDays { get; set; } 
		public string WorkingHours { get; set; } 
		public bool IsActive { get; set; } 
		public int CompletedSessions { get; set; } = 0;

	}
}
