using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.NutritionModule.Entities
{
	public class FoodItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public float Calories { get; set; } // per 100g or per serving
		public float Protein { get; set; }
		public float Carbs { get; set; }
		public float Fats { get; set; }
		public string? Category { get; set; } // e.g., "Protein", "Vegetable"
		public bool IsVegetarian { get; set; }
		public bool IsVegan { get; set; }
		public bool IsGlutenFree { get; set; }

		public virtual ICollection<MealItem> MealItems { get; set; }


	}
}
