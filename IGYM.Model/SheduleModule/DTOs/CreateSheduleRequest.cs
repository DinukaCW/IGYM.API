using Microsoft.Identity.Client;
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
		public int TrainerId { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public decimal? Height { get; set; } // in cm
		public decimal Weight { get; set; } // in kg
		public DateTime StartDate { get; set; } // Start date of the schedule
		public DateTime EndDate { get; set; } // End date of the schedule
		public string Goal { get; set; } // e.g., weight loss, muscle gain, etc.
		public string FitnessLevel { get; set; } // e.g., beginner, intermediate, advanced
		public string TrainingType { get; set; } // e.g., strength training, cardio, etc.
		public string MedicalConditions { get; set; } // e.g., diabetes, hypertension, etc.
		public string Notes { get; set; }
		
	}
}
