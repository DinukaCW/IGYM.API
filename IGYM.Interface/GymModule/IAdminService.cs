using IGYM.Model.AdminModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface.GymModule
{
	public interface IAdminService
	{
		Task<WorkoutResponseDto> AddWorkoutAsync(AddWorkoutDto workoutDto);
		Task<(int TrainerId, int UserId)> AddTrainerAsync(AddTrainerDto trainerDto);
		Task<ServiceResult> DeleteTrainerAsync(int trainerId);
		Task<ServiceResult> DeleteWorkoutAsync(int workoutId);
		Task<FoodItemDto> AddFoodItemAsync(CreateFoodItemDto foodItemDto);
		Task<ServiceResult> DeleteFoodItemAsync(int foodItemId);
	}
}
