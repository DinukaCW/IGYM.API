using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.SheduleModule.Entities
{
	public class Trainer
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TrainerId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(100)]
		public string Specialization { get; set; }

		[Column(TypeName = "decimal(10,2)")]
		public decimal HourlyRate { get; set; }

		[StringLength(100)]
		public string AvailableDays { get; set; } // 'Mon,Wed,Fri' or 'Tue,Thu,Sat'

		[StringLength(100)]
		public string WorkingHours { get; set; } // '09:00-17:00' or '14:00-22:00'

		public bool Active { get; set; } = true;
	 
	}
}
