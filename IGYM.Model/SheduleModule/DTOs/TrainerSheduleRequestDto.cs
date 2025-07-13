using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.DTOs
{
	public class TrainerSheduleRequestDto
	{
		public int MemberSheduleRequestId { get; set; }
		public string MemberName { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public decimal? Height { get; set; }
		public decimal Weight { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Goal { get; set; }
		public string FitnessLevel { get; set; }
		public string TrainingType { get; set; }
		public string MedicalConditions { get; set; }
		public string Notes { get; set; }
		public DateTime RequestDate { get; set; }
		public string RequestStatus { get; set; }
	}
}
