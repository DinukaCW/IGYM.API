using Google;
using IGYM.Interface.GymModule;
using IGYM.Model;
using IGYM.Model.AdminModule;
using IGYM.Model.SheduleModule.DTOs;
using IGYM.Model.SheduleModule.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Service.GymModule
{
	
	public class GymScheduleService : IGymSheduleService
	{
		private readonly IGYMDbContext _context;
		private readonly ILogger<GymScheduleService> _logger;

		public GymScheduleService(IGYMDbContext context, ILogger<GymScheduleService> logger)
		{
			_context = context;
			_logger = logger;
		}


		/// <summary>
		/// Creates a new schedule request from a member to a trainer.
		/// </summary>
		/// <param name="request">The schedule request details.</param>
		/// <returns>The ID of the created schedule request.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the request parameter is null.</exception>
		/// <exception cref="DbUpdateException">Thrown when there's an error saving to the database.</exception>
		public async Task<int> CreateScheduleRequestAsync(CreateSheduleRequest request)
		{
			try
			{
				// Validate input
				if (request == null)
				{
					throw new ArgumentNullException(nameof(request), "Schedule request cannot be null.");
				}

				// Create new schedule request entity
				var scheduleRequest = new MemberSheduleRequest
				{
					MemberId = request.MemberId,
					TrainerId = request.TrainerId,
					Age = request.Age,
					Gender = request.Gender,
					Height = request.Height,
					Weight = request.Weight,
					StartDate = request.StartDate,
					EndDate = request.EndDate,
					Goal = request.Goal,
					FitnessLevel = request.FitnessLevel,
					TrainingType = request.TrainingType,
					MedicalConditions = request.MedicalConditions,
					Notes = request.Notes,
					RequestDate = DateTime.UtcNow,
					RequestStatus = "pending"
				};

				// Add to context and save changes
				_context.MemberSheduleRequest.Add(scheduleRequest);
				await _context.SaveChangesAsync();

				return scheduleRequest.MemberSheduleRequestId;
			}
			catch (DbUpdateException ex)
			{
				// Log the database error
				throw new DbUpdateException("Error saving schedule request to database.", ex);
			}
		}

		/// <summary>
		/// Retrieves all pending schedule requests for a specific trainer.
		/// </summary>
		/// <param name="trainerId">The ID of the trainer.</param>
		/// <returns>A list of pending schedule requests.</returns>
		/// <exception cref="ArgumentException">Thrown when trainerId is less than or equal to 0.</exception>
		public async Task<List<TrainerSheduleRequestDto>> GetTrainerPendingRequestsAsync(int trainerId)
		{
			try
			{
				// Validate input
				if (trainerId <= 0)
				{
					throw new ArgumentException("Trainer ID must be greater than 0.", nameof(trainerId));
				}

				// Query pending requests for the specified trainer
				return await _context.MemberSheduleRequest
					.Where(r => r.TrainerId == trainerId && r.RequestStatus == "pending")
					.Include(r => r.Member)
					.Select(r => new TrainerSheduleRequestDto
					{
						MemberSheduleRequestId = r.MemberSheduleRequestId,
						MemberName = r.Member.Name,
						Age = r.Age,
						Gender = r.Gender,
						Height = r.Height,
						Weight = r.Weight,
						StartDate = r.StartDate,
						EndDate = r.EndDate,
						Goal = r.Goal,
						FitnessLevel = r.FitnessLevel,
						TrainingType = r.TrainingType,
						MedicalConditions = r.MedicalConditions,
						Notes = r.Notes,
						RequestDate = r.RequestDate,
						RequestStatus = r.RequestStatus
					})
					.OrderBy(r => r.RequestDate) // Oldest requests first
					.ToListAsync();
			}
			catch (Exception ex)
			{
				// Log the error and rethrow
				throw new Exception("Error retrieving trainer pending requests.", ex);
			}
		}

		/// <summary>
		/// Creates a workout plan based on a schedule request.
		/// </summary>
		/// <param name="planDto">The workout plan details.</param>
		/// <returns>True if the plan was created successfully, false otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown when planDto is null.</exception>
		public async Task<bool> CreateWorkoutPlanAsync(CreateWorkoutPlanDto planDto)
		{
			// Validate input
			if (planDto == null)
			{
				throw new ArgumentNullException(nameof(planDto), "Workout plan DTO cannot be null.");
			}

			// Start a transaction to ensure data consistency
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// Find the schedule request
				var scheduleRequest = await _context.MemberSheduleRequest
					.FirstOrDefaultAsync(r => r.MemberSheduleRequestId == planDto.MembersheduleRequestId);

				if (scheduleRequest == null)
				{
					// Request not found
					return false;
				}

				// Create the main schedule
				var memberSchedule = new MemberShedule
				{
					MemberId = scheduleRequest.MemberId,
					TrainerId = scheduleRequest.TrainerId,
					MembersheduleRequestId = planDto.MembersheduleRequestId,
					PlanName = planDto.PlanName,
					CreateDate = DateTime.UtcNow,
					StartTime = planDto.StartTime,
					EndTime = planDto.EndTime,
					Status = PlanStatus.Active,
					Notes = planDto.Notes
				};

				// Add to context
				_context.MemberShedule.Add(memberSchedule);
				await _context.SaveChangesAsync();

				// Add all scheduled workouts from the plan
				foreach (var dailyPlan in planDto.DailyWorkouts)
				{
					foreach (var workout in dailyPlan.Workouts)
					{
						var scheduledWorkout = new SheduleWorkout
						{
							ScheduleId = memberSchedule.ScheduleId,
							DayNumber = dailyPlan.DayNumber,
							WorkoutId = workout.WorkoutId,
							SequenceOrder = workout.SequenceOrder,
							DurationMinutes = workout.DurationMinutes,
							RestMinutes = workout.RestMinutes,
							Completed = false,
							Notes = workout.Notes
						};

						_context.SheduleWorkout.Add(scheduledWorkout);
					}
				}

				// Update the original request status to approved
				scheduleRequest.RequestStatus = "approved";

				// Save all changes and commit transaction
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return true;
			}
			catch (Exception ex)
			{
				// Rollback transaction on error
				await transaction.RollbackAsync();

				// Log the error
				throw new Exception("Error creating workout plan.", ex);
			}
		}

		/// <summary>
		/// Retrieves all workout plans for a specific member.
		/// </summary>
		/// <param name="memberId">The ID of the member.</param>
		/// <returns>A list of workout plans for the member.</returns>
		/// <exception cref="ArgumentException">Thrown when memberId is less than or equal to 0.</exception>
		public async Task<List<MemberWorkoutPlanDto>> GetMemberWorkoutPlansAsync(int memberId)
		{
			try
			{
				// Validate input
				if (memberId <= 0)
				{
					throw new ArgumentException("Member ID must be greater than 0.", nameof(memberId));
				}

				// Query all workout plans for the member
				return await _context.MemberShedule
					.Where(s => s.MemberId == memberId)
					.Include(s => s.Trainer)
					.Include(s => s.ScheduledWorkouts)
						.ThenInclude(sw => sw.Workout)
					.Select(s => new MemberWorkoutPlanDto
					{
						ScheduleId = s.ScheduleId,
						PlanName = s.PlanName,
						TrainerName = s.Trainer.Name,
						StartTime = s.StartTime,
						EndTime = s.EndTime,
						Status = s.Status,
						Notes = s.Notes,
						CreateDate = s.CreateDate,
						DailyWorkouts = s.ScheduledWorkouts
							.GroupBy(sw => sw.DayNumber)
							.Select(g => new DailyWorkoutViewDto
							{
								DayNumber = g.Key,
								TotalDurationMinutes = g.Sum(sw => sw.DurationMinutes),
								IsCompleted = g.All(sw => sw.Completed),
								Workouts = g.OrderBy(sw => sw.SequenceOrder)
									.Select(sw => new WorkoutDetailDto
									{
										ScheduledWorkoutId = sw.ScheduledWorkoutId,
										WorkoutId = sw.WorkoutId,
										WorkoutName = sw.Workout.Name,
										Description = sw.Workout.Description,
										Category = sw.Workout.Category,
										Difficulty = sw.Workout.Difficulty,
										EquipmentNeeded = sw.Workout.EquipmentNeeded,
										Image = sw.Workout.Image,
										SequenceOrder = sw.SequenceOrder,
										DurationMinutes = sw.DurationMinutes,
										RestMinutes = sw.RestMinutes,
										Completed = sw.Completed,
										Notes = sw.Notes
									}).ToList()
							})
							.OrderBy(d => d.DayNumber)
							.ToList()
					})
					.OrderByDescending(s => s.CreateDate) // Most recent plans first
					.ToListAsync();
			}
			catch (Exception ex)
			{
				// Log the error and rethrow
				throw new Exception("Error retrieving member workout plans.", ex);
			}
		}

		/// <summary>
		/// Retrieves a specific workout plan for a member by schedule ID
		/// </summary>
		/// <param name="memberId">The ID of the member</param>
		/// <param name="scheduleId">The ID of the workout plan</param>
		/// <returns>The requested workout plan, or null if not found</returns>
		/// <exception cref="ArgumentException">Thrown when invalid IDs are provided</exception>
		/// <exception cref="InvalidOperationException">Thrown when the workout plan cannot be retrieved</exception>
		public async Task<MemberWorkoutPlanDto> GetMemberWorkoutPlanByIdAsync(int memberId, int scheduleId)
		{
			try
			{
				// Validate inputs
				if (memberId <= 0)
				{
					throw new ArgumentException("Member ID must be greater than 0.", nameof(memberId));
				}

				if (scheduleId <= 0)
				{
					throw new ArgumentException("Schedule ID must be greater than 0.", nameof(scheduleId));
				}

				// Query the specific workout plan with all required includes
				var workoutPlan = await _context.MemberShedule
					.AsNoTracking() // Improves performance since we're only reading data
					.Where(s => s.MemberId == memberId && s.ScheduleId == scheduleId)
					.Include(s => s.Trainer)
					.Include(s => s.ScheduledWorkouts)
						.ThenInclude(sw => sw.Workout)
					.FirstOrDefaultAsync();

				if (workoutPlan == null)
				{
					return null; // Return null if no matching plan found
				}

				// Map the entity to DTO
				return new MemberWorkoutPlanDto
				{
					ScheduleId = workoutPlan.ScheduleId,
					PlanName = workoutPlan.PlanName,
					TrainerName = workoutPlan.Trainer?.Name ?? "Unknown Trainer", // Use Name property directly
					StartTime = workoutPlan.StartTime,
					EndTime = workoutPlan.EndTime,
					Status = workoutPlan.Status,
					Notes = workoutPlan.Notes,
					CreateDate = workoutPlan.CreateDate,
					DailyWorkouts = workoutPlan.ScheduledWorkouts?
						.GroupBy(sw => sw.DayNumber)
						.Select(g => new DailyWorkoutViewDto
						{
							DayNumber = g.Key,
							TotalDurationMinutes = g.Sum(sw => sw.DurationMinutes),
							IsCompleted = g.All(sw => sw.Completed),
							Workouts = g.OrderBy(sw => sw.SequenceOrder)
								.Select(sw => new WorkoutDetailDto
								{
									ScheduledWorkoutId = sw.ScheduledWorkoutId,
									WorkoutId = sw.WorkoutId,
									WorkoutName = sw.Workout?.Name ?? "Unknown Workout",
									Description = sw.Workout?.Description,
									Category = sw.Workout?.Category,
									Difficulty = sw.Workout?.Difficulty,
									EquipmentNeeded = sw.Workout?.EquipmentNeeded,
									Image = sw.Workout?.Image,
									SequenceOrder = sw.SequenceOrder,
									DurationMinutes = sw.DurationMinutes,
									RestMinutes = sw.RestMinutes,
									Completed = sw.Completed,
									Notes = sw.Notes
								})
								.ToList()
						})
						.OrderBy(d => d.DayNumber)
						.ToList() ?? new List<DailyWorkoutViewDto>()
				};
			}
			catch (ArgumentException)
			{
				// Re-throw argument exceptions as they're already properly formatted
				throw;
			}
			catch (Exception ex)
			{
				// Log the specific error details here
				throw new InvalidOperationException($"Error retrieving workout plan {scheduleId} for member {memberId}.", ex);
			}
		}

		/// <summary>
		/// Retrieves the workouts scheduled for a specific day of a workout plan.
		/// </summary>
		/// <param name="memberId">The ID of the member.</param>
		/// <param name="scheduleId">The ID of the schedule/plan.</param>
		/// <param name="dayNumber">The day number within the plan.</param>
		/// <returns>A list of workouts for the specified day.</returns>
		/// <exception cref="ArgumentException">Thrown when any parameter is invalid.</exception>
		public async Task<List<WorkoutDetailDto>> GetDailyWorkoutsAsync(int memberId, int scheduleId, int dayNumber)
		{
			try
			{
				// Validate inputs
				if (memberId <= 0)
				{
					throw new ArgumentException("Member ID must be greater than 0.", nameof(memberId));
				}

				if (scheduleId <= 0)
				{
					throw new ArgumentException("Schedule ID must be greater than 0.", nameof(scheduleId));
				}

				if (dayNumber <= 0)
				{
					throw new ArgumentException("Day number must be greater than 0.", nameof(dayNumber));
				}

				// Query workouts for the specified day
				return await _context.SheduleWorkout
					.Where(sw => sw.MemberSchedule.MemberId == memberId &&
								sw.ScheduleId == scheduleId &&
								sw.DayNumber == dayNumber)
					.Include(sw => sw.Workout)
					.OrderBy(sw => sw.SequenceOrder)
					.Select(sw => new WorkoutDetailDto
					{
						ScheduledWorkoutId = sw.ScheduledWorkoutId,
						WorkoutId = sw.WorkoutId,
						WorkoutName = sw.Workout.Name,
						Description = sw.Workout.Description,
						Category = sw.Workout.Category,
						Difficulty = sw.Workout.Difficulty,
						EquipmentNeeded = sw.Workout.EquipmentNeeded,
						Image = sw.Workout.Image,
						SequenceOrder = sw.SequenceOrder,
						DurationMinutes = sw.DurationMinutes,
						RestMinutes = sw.RestMinutes,
						Completed = sw.Completed,
						Notes = sw.Notes
					})
					.ToListAsync();
			}
			catch (Exception ex)
			{
				// Log the error and rethrow
				throw new Exception("Error retrieving daily workouts.", ex);
			}
		}

		/// <summary>
		/// Updates the completion status of multiple workouts.
		/// </summary>
		/// <param name="memberId">The ID of the member.</param>
		/// <param name="completions">List of workout completion statuses to update.</param>
		/// <returns>True if updates were successful, false otherwise.</returns>
		/// <exception cref="ArgumentException">Thrown when memberId is invalid or completions is null/empty.</exception>
		public async Task<bool> UpdateWorkoutCompletionAsync(int memberId, List<WorkOutCompletionDto> completions)
		{
			try
			{
				// Validate inputs
				if (memberId <= 0)
				{
					throw new ArgumentException("Member ID must be greater than 0.", nameof(memberId));
				}

				if (completions == null || !completions.Any())
				{
					throw new ArgumentException("Completion list cannot be null or empty.", nameof(completions));
				}

				// Get IDs of workouts to update
				var scheduledWorkoutIds = completions.Select(c => c.ScheduledWorkoutId).ToList();

				// Find all relevant scheduled workouts
				var scheduledWorkouts = await _context.SheduleWorkout
					.Where(sw => scheduledWorkoutIds.Contains(sw.ScheduledWorkoutId) &&
								sw.MemberSchedule.MemberId == memberId)
					.ToListAsync();

				// Update each workout's completion status
				foreach (var completion in completions)
				{
					var scheduledWorkout = scheduledWorkouts
						.FirstOrDefault(sw => sw.ScheduledWorkoutId == completion.ScheduledWorkoutId);

					if (scheduledWorkout != null)
					{
						scheduledWorkout.Completed = completion.Completed;
						if (!string.IsNullOrEmpty(completion.Notes))
						{
							scheduledWorkout.Notes = completion.Notes;
						}
					}
				}

				// Save changes
				await _context.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				// Log the error
				throw new Exception("Error updating workout completions.", ex);
			}
		}

		/// <summary>
		/// Updates the status of a workout schedule/plan.
		/// </summary>
		/// <param name="scheduleId">The ID of the schedule to update.</param>
		/// <param name="status">The new status to set.</param>
		/// <returns>True if update was successful, false if schedule was not found.</returns>
		/// <exception cref="ArgumentException">Thrown when scheduleId is invalid.</exception>
		public async Task<bool> UpdateScheduleStatusAsync(int scheduleId, PlanStatus status)
		{
			try
			{
				// Validate input
				if (scheduleId <= 0)
				{
					throw new ArgumentException("Schedule ID must be greater than 0.", nameof(scheduleId));
				}

				// Find the schedule
				var schedule = await _context.MemberShedule
					.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

				if (schedule == null)
				{
					return false;
				}

				// Update status and save
				schedule.Status = status;
				await _context.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				// Log the error
				throw new Exception("Error updating schedule status.", ex);
			}
		}

		/// <summary>
		/// Updates the status of a schedule request.
		/// </summary>
		/// <param name="requestId">The ID of the request to update.</param>
		/// <param name="status">The new status to set.</param>
		/// <returns>True if update was successful, false if request was not found.</returns>
		/// <exception cref="ArgumentException">Thrown when requestId is invalid.</exception>
		public async Task<bool> UpdateRequestStatusAsync(int requestId, string status)
		{
			try
			{
				// Validate input
				if (requestId <= 0)
				{
					throw new ArgumentException("Request ID must be greater than 0.", nameof(requestId));
				}

				// Find the request
				var request = await _context.MemberSheduleRequest
					.FirstOrDefaultAsync(r => r.MemberSheduleRequestId == requestId);

				if (request == null)
				{
					return false;
				}

				// Update status and save
				request.RequestStatus = status;
				await _context.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				// Log the error
				throw new Exception("Error updating request status.", ex);
			}
		}

		/// <summary>
		/// Retrieves all available workouts in the system.
		/// </summary>
		/// <returns>A list of all available workouts, ordered by category and name.</returns>
		public async Task<List<WorkoutResponseDto>> GetAvailableWorkoutsAsync()
		{

				try
				{
					var workouts = await _context.Workout
						.AsNoTracking()
						.OrderBy(w => w.Name)
						.Select(w => new WorkoutResponseDto
						{
							WorkoutId = w.WorkoutId,
							Name = w.Name,
							Description = w.Description,
							Category = w.Category,
							DurationMinutes = w.DurationMinutes,
							Difficulty = w.Difficulty,
							EquipmentNeeded = w.EquipmentNeeded,
							Image = w.Image != null ? Convert.ToBase64String(w.Image) : null
						})
						.ToListAsync();

					return workouts;
				}
				catch (Exception ex)
				{
					// Log the error
					throw new Exception("Error retrieving available workouts.", ex);
				}
			}


		/// <summary>
		/// Gets all trainers with their availability information
		/// </summary>
		public async Task<List<TrainerDto>> GetTrainersAsync(bool activeOnly = true, string specialization = null)
		{
			var query = _context.Set<Trainer>().AsQueryable();

			if (activeOnly)
				query = query.Where(t => t.Active);

			if (!string.IsNullOrEmpty(specialization))
				query = query.Where(t => t.Specialization.ToLower().Contains(specialization.ToLower()));

			var trainers = await query
				.OrderBy(t => t.Name)
				.Select(t => new TrainerDto
				{
					TrainerId = t.TrainerId,
					Name = t.Name,
					Specialization = t.Specialization,
					HourlyRate = t.HourlyRate,
					AvailableDays = t.AvailableDays,
					WorkingHours = t.WorkingHours,
					IsActive = t.Active,
					CompletedSessions = _context.Set<MemberShedule>()
						.Count(sw => sw.TrainerId == t.TrainerId && sw.Status == PlanStatus.Completed)
				})
				.ToListAsync();

			return trainers;
		}


		

	}

}
