using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model.AdminModule
{
	public class FoodItemDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public float Calories { get; set; }
		public float Protein { get; set; }
		public float Carbs { get; set; }
		public float Fats { get; set; }
		public string Category { get; set; }
		public bool IsVegetarian { get; set; }
		public bool IsVegan { get; set; }
		public bool IsGlutenFree { get; set; }
	}
}
