using IGYM.Model.UserModule.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.Entities
{
	public class MemberSheduleRequest
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int MemberSheduleRequestId { get; set; }
		public int MemberId { get; set; }
		public int TrainerId { get; set; }
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
		public DateTime RequestDate { get; set; } = DateTime.UtcNow;
		public string RequestStatus { get; set; } = "pending";

		// Navigation properties
		public virtual User Member { get; set; }
		public virtual Trainer Trainer { get; set; }
		public virtual ICollection<MemberShedule> MemberSchedules { get; set; }
	}

	public enum RequestStatus
	{
		Pending,
		InProgress,
		Completed,
		Cancelled
	}
}
