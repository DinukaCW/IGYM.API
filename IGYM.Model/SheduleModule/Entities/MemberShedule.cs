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
	public class MemberShedule
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ScheduleId { get; set; }
		public int MemberId { get; set; }
		[Required]
		public int TrainerId { get; set; }
		public int MembersheduleRequestId { get; set; }
		public string PlanName { get; set; }
		public DateTime CreateDate { get; set; } = DateTime.UtcNow;
		[Required]
		public DateTime StartTime { get; set; }
		[Required]
		public DateTime EndTime { get; set; }
		[StringLength(20)]
		public PlanStatus Status { get; set; } = PlanStatus.Active;
		public string Notes { get; set; }

		// Navigation properties
		public virtual MemberSheduleRequest ScheduleRequest { get; set; }
		public virtual User Member { get; set; }
		public virtual Trainer Trainer { get; set; }
		public virtual ICollection<SheduleWorkout> ScheduledWorkouts { get; set; }

	}

	public enum PlanStatus
	{
		Active,
		Completed,
		Paused,
		Cancelled
	}
}
