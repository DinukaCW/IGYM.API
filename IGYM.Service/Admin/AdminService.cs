using IGYM.Interface;
using IGYM.Interface.Data;
using IGYM.Interface.UserModule;
using IGYM.Model;
using IGYM.Model.AdminModule;
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
		public record SeResult(bool Success, string Message);



	}
}
