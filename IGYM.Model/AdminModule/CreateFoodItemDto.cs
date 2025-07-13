using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.AdminModule
{
	public class CreateFoodItemDto
	{
		[Required]
		[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
		public string Name { get; set; }

		[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
		public string Description { get; set; }

		[Required]
		[Range(0, float.MaxValue, ErrorMessage = "Calories must be positive")]
		public float Calories { get; set; }

		[Required]
		[Range(0, float.MaxValue, ErrorMessage = "Protein must be positive")]
		public float Protein { get; set; }

		[Required]
		[Range(0, float.MaxValue, ErrorMessage = "Carbs must be positive")]
		public float Carbs { get; set; }

		[Required]
		[Range(0, float.MaxValue, ErrorMessage = "Fats must be positive")]
		public float Fats { get; set; }

		[StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
		public string Category { get; set; }

		public bool IsVegetarian { get; set; }
		public bool IsVegan { get; set; }
		public bool IsGlutenFree { get; set; }
	}
}
