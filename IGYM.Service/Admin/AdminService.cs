using IGYM.Interface;
using IGYM.Interface.Data;
using IGYM.Interface.UserModule;
using IGYM.Model;
using IGYM.Model.AdminModule;
using IGYM.Model.NutritionModule.Entities;
using IGYM.Model.SheduleModule.Entities;
using IGYM.Model.UserModule.Entities;
using IGYM.Service.UserModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Service.Admin
{
	public class AdminService
	{
		private readonly IEncryptionService _encryption;
		private readonly ILogger<AdminService> _logger;
		private readonly IGYMDbContext _context;
		private readonly IConfiguration _configuration;
		public AdminService(IEncryptionService encryption, IConfiguration configuration, ILogger<AdminService> logger, IGYMDbContext dbContext)
		{
			_encryption = encryption;
			_logger = logger;
			_context = dbContext;
			_configuration = configuration;

		}

		public async Task<WorkoutResponseDto> AddWorkoutAsync(AddWorkoutDto workoutDto)
		{
			try
			{
				if (workoutDto == null)
				{
					throw new ArgumentNullException(nameof(workoutDto), "Workout data cannot be null.");
				}

				byte[]? imageBytes = null;
				if (workoutDto.ImageFile != null && workoutDto.ImageFile.Length > 0)
				{
					using var memoryStream = new MemoryStream();
					await workoutDto.ImageFile.CopyToAsync(memoryStream);
					imageBytes = memoryStream.ToArray();

					// Validate image size (e.g., max 5MB)
					if (imageBytes.Length > 5 * 1024 * 1024)
					{
						throw new ArgumentException("Image size cannot exceed 5MB.");
					}
				}

				var workout = new Workout
				{
					Name = workoutDto.Name,
					Description = workoutDto.Description,
					Category = workoutDto.Category,
					DurationMinutes = workoutDto.DurationMinutes,
					Difficulty = workoutDto.Difficulty,
					EquipmentNeeded = workoutDto.EquipmentNeeded,
					Image = imageBytes
				};

				_context.Workout.Add(workout);
				await _context.SaveChangesAsync();

				return new WorkoutResponseDto
				{
					WorkoutId = workout.WorkoutId,
					Name = workout.Name,
					Description = workout.Description,
					Category = workout.Category,
					DurationMinutes = workout.DurationMinutes,
					Difficulty = workout.Difficulty,
					EquipmentNeeded = workout.EquipmentNeeded,
					Image = workout.Image != null ? Convert.ToBase64String(workout.Image) : null
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding new workout");
				throw;
			}
		}

		/// <summary>
		/// Adds a new trainer and creates their login account
		/// </summary>
		/// <param name="trainerDto">Trainer details</param>
		/// <param name="password">Initial password</param>
		/// <returns>Tuple containing TrainerId and UserId</returns>
		public async Task<(int TrainerId, int UserId)> AddTrainerAsync(AddTrainerDto trainerDto)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 1. Create user account first
				var user = new User
				{
					Username = _encryption.EncryptData(trainerDto.Username),
					Name = trainerDto.Name,
					Email = _encryption.EncryptData(trainerDto.Email),
					PhoneNumber = _encryption.EncryptData(trainerDto.PhoneNumber),
					UserRoleId = 2, // Should be Trainer role ID
					PasswordHash = _encryption.EncryptData(trainerDto.Password),
					IsActive = true
				};

				_context.Users.Add(user);
				await _context.SaveChangesAsync();

				// 2. Create trainer profile
				var trainer = new Trainer
				{
					Name = trainerDto.Name,
					Specialization = trainerDto.Specialization,
					HourlyRate = trainerDto.HourlyRate,
					AvailableDays = trainerDto.AvailableDays,
					WorkingHours = trainerDto.WorkingHours,
					Active = true,
					UserId = user.UserID // Link to user account
				};

				_context.Trainer.Add(trainer);
				await _context.SaveChangesAsync();

				await transaction.CommitAsync();

				return (trainer.TrainerId, user.UserID);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error adding new trainer");
				throw;
			}
		}

		public async Task<ServiceResult> DeleteTrainerAsync(int trainerId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 1. Find the trainer with related data
				var trainer = await _context.Trainer
					.Include(t => t.User)
					.FirstOrDefaultAsync(t => t.TrainerId == trainerId);

				if (trainer == null)
				{
					return new ServiceResult(false, "Trainer not found");
				}

				var memberSchedules = await _context.MemberShedule
					.Include(s => s.Trainer)
					.Where(s => s.TrainerId == trainerId)
					.ToListAsync();
				// 2. Check for active schedules
				if (memberSchedules != null && memberSchedules.Any(s => s.Status == PlanStatus.Active))
				{
					return new ServiceResult(false, "Cannot delete trainer with active schedules");
				}

				// 3. Soft delete the user account
				if (trainer.User != null)
				{
					trainer.User.IsActive = false;
					trainer.User.IsLocked = true;
					_context.Users.Update(trainer.User);
				}

				// 4. Soft delete the trainer profile
				trainer.Active = false;
				_context.Trainer.Update(trainer);

				// 5. Archive related data (optional)
				var schedules = await _context.MemberShedule
					.Where(s => s.TrainerId == trainerId)
					.ToListAsync();

				foreach (var s in schedules)
				{
					s.Status = PlanStatus.Cancelled;
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new ServiceResult(true, "Trainer deactivated successfully");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error deleting trainer {TrainerId}", trainerId);
				throw new ApplicationException("An error occurred while deleting trainer", ex);
			}
		}

		public async Task<ServiceResult> DeleteWorkoutAsync(int workoutId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 1. Find workout with scheduled instances
				var workout = await _context.Workout
					.Include(w => w.ScheduledWorkouts)
					.FirstOrDefaultAsync(w => w.WorkoutId == workoutId);

				if (workout == null)
				{
					return new ServiceResult(false, "Workout not found");
				}

				// 2. Check if workout is in any active schedules
				var hasActiveSchedules = await _context.MemberShedule
					.Include(ms => ms.ScheduledWorkouts)
					.AnyAsync(ms => ms.Status == PlanStatus.Active &&
									ms.ScheduledWorkouts.Any(sw => sw.WorkoutId == workoutId));

				if (hasActiveSchedules)
				{
					return new ServiceResult(false,
						"Cannot delete workout that's part of active schedules");
				}

				// 3. Archive the workout (soft delete pattern)
				if (workout.GetType().GetProperty("IsActive") != null)
				{
					workout.GetType().GetProperty("IsActive")?.SetValue(workout, false);
					_context.Workout.Update(workout);
				}

				// 4. Remove from future schedules (optional)
				var futureScheduledWorkouts = await _context.SheduleWorkout
					.Include(sw => sw.MemberSchedule)
					.Where(sw =>
						sw.WorkoutId == workoutId &&
						sw.MemberSchedule != null &&
						sw.MemberSchedule.StartTime > DateTime.UtcNow)
					.ToListAsync();

				_context.SheduleWorkout.RemoveRange(futureScheduledWorkouts);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new ServiceResult(true,
					$"Workout deleted. Removed from {futureScheduledWorkouts.Count} future schedules");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error deleting workout {WorkoutId}", workoutId);
				throw new ApplicationException("An error occurred while deleting workout", ex);
			}
		}

		public async Task<FoodItemDto> AddFoodItemAsync(CreateFoodItemDto foodItemDto)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// Validate input
				if (foodItemDto == null)
				{
					throw new ArgumentNullException(nameof(foodItemDto), "Food item data cannot be null");
				}

				// Validate nutritional values
				if (foodItemDto.Calories < 0)
				{
					throw new ArgumentException("Calories cannot be negative", nameof(foodItemDto.Calories));
				}

				if (foodItemDto.Protein < 0)
				{
					throw new ArgumentException("Protein cannot be negative", nameof(foodItemDto.Protein));
				}

				if (foodItemDto.Carbs < 0)
				{
					throw new ArgumentException("Carbs cannot be negative", nameof(foodItemDto.Carbs));
				}

				if (foodItemDto.Fats < 0)
				{
					throw new ArgumentException("Fats cannot be negative", nameof(foodItemDto.Fats));
				}

				// Check for duplicate food item names
				var existingItem = await _context.FoodItem
					.FirstOrDefaultAsync(f => f.Name.ToLower() == foodItemDto.Name.ToLower());

				if (existingItem != null)
				{
					throw new InvalidOperationException($"Food item with name '{foodItemDto.Name}' already exists");
				}

				// Create new food item
				var foodItem = new FoodItem
				{
					Name = foodItemDto.Name,
					Description = foodItemDto.Description,
					Calories = foodItemDto.Calories,
					Protein = foodItemDto.Protein,
					Carbs = foodItemDto.Carbs,
					Fats = foodItemDto.Fats,
					Category = foodItemDto.Category,
					IsVegetarian = foodItemDto.IsVegetarian,
					IsVegan = foodItemDto.IsVegan,
					IsGlutenFree = foodItemDto.IsGlutenFree
				};

				_context.FoodItem.Add(foodItem);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new FoodItemDto
				{
					Id = foodItem.Id,
					Name = foodItem.Name,
					Description = foodItem.Description,
					Calories = foodItem.Calories,
					Protein = foodItem.Protein,
					Carbs = foodItem.Carbs,
					Fats = foodItem.Fats,
					Category = foodItem.Category,
					IsVegetarian = foodItem.IsVegetarian,
					IsVegan = foodItem.IsVegan,
					IsGlutenFree = foodItem.IsGlutenFree
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error adding new food item");
				throw;
			}
		}

		public async Task<ServiceResult> DeleteFoodItemAsync(int foodItemId)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// 1. Find food item with related meal items
				var foodItem = await _context.FoodItem
					.Include(f => f.MealItems)
						.ThenInclude(mi => mi.MealPlan)
							.ThenInclude(mp => mp.NutritionPlan)
								.ThenInclude(np => np.Request)
					.FirstOrDefaultAsync(f => f.Id == foodItemId);

				if (foodItem == null)
				{
					return new ServiceResult(false, "Food item not found");
				}

				// 2. Check if food item is used in any completed nutrition plans
				var hasCompletedPlans = foodItem.MealItems.Any(mi =>
					mi.MealPlan?.NutritionPlan?.Request?.Status == NutritionPlanRequestStatus.Completed);

				if (hasCompletedPlans)
				{
					return new ServiceResult(false,
						"Cannot delete food item that's part of completed nutrition plans");
				}

				// 3. Get all pending/active meal items (not part of completed plans)
				var mealItemsToRemove = foodItem.MealItems
					.Where(mi => mi.MealPlan?.NutritionPlan?.Request?.Status != NutritionPlanRequestStatus.Completed)
					.ToList();

				// 4. Remove the meal items
				_context.MealItem.RemoveRange(mealItemsToRemove);

				// 5. Only hard delete if not used in any plans
				if (!foodItem.MealItems.Any(mi =>
					mi.MealPlan?.NutritionPlan?.Request?.Status == NutritionPlanRequestStatus.Completed))
				{
					_context.FoodItem.Remove(foodItem);
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return new ServiceResult(true,
					foodItem.MealItems.Any()
						? "Food item removed from pending/active plans but kept in completed plans"
						: "Food item completely deleted");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error deleting food item {FoodItemId}", foodItemId);
				throw new ApplicationException("An error occurred while deleting food item", ex);
			}
		}
		public record SeResult(bool Success, string Message);



	}
}
