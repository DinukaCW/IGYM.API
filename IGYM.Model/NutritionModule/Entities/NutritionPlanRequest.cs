using IGYM.Model.SheduleModule.Entities;
using IGYM.Model.UserModule.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.Entities
{
	public class NutritionPlanRequest
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int MemberId { get; set; }
		public int TrainerId { get; set; }
		public string Goal { get; set; }
		public float Weight { get; set; }
		public float Height { get; set; }
		public string DietPreference { get; set; }
		public string MedicalNotes { get; set; }
		public string AdditionalNotes { get; set; }
		public DateTime RequestDate { get; set; } = DateTime.UtcNow;
		public NutritionPlanRequestStatus Status { get; set; } = NutritionPlanRequestStatus.Pending;

			// Navigation properties
		public virtual User Member { get; set; }
		public virtual Trainer Trainer { get; set; }
		public virtual NutritionPlan? NutritionPlan { get; set; }
	}

		public enum NutritionPlanRequestStatus
		{
			Pending,
			InProgress,
			Completed,
			Rejected
		}
	}
