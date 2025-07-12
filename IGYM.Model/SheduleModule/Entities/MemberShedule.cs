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

		public DateTime Date { get; set; }

		[Required]
		public TimeSpan StartTime { get; set; }

		[Required]
		public TimeSpan EndTime { get; set; }

		[StringLength(20)]
		public string Status { get; set; } = "planned"; // planned, completed, canceled

		public string Notes { get; set; }

	}
}
