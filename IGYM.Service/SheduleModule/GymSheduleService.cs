using IGYM.Interface.SheduleModule;
using IGYM.Model;
using IGYM.Model.SheduleModule.DTOs;
using IGYM.Model.SheduleModule.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Service.SheduleModule
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
		/// Creates a new schedule for a member with selected workouts
		/// </summary>
		public async Task<SheduleResult> CreateMemberScheduleAsync(CreateSheduleRequest request)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// Validate time slot availability
				var isTimeSlotAvailable = await IsTimeSlotAvailableAsync(request.MemberId, request.Date, request.StartTime, request.EndTime);
				if (!isTimeSlotAvailable)
				{
					return new SheduleResult
					{
						Success = false,
						Message = "Time slot is already booked for this member."
					};
				}

				// Create main schedule
				var schedule = new MemberShedule
				{
					MemberId = request.MemberId,
					TrainerId = request.TrainerId,
					Date = request.Date,
					StartTime = request.StartTime,
					EndTime = request.EndTime,
					Status = "planned",
					Notes = request.Notes
				};

				_context.Set<MemberShedule>().Add(schedule);
				await _context.SaveChangesAsync();

				// Add workouts to schedule
				var scheduledWorkouts = new List<SheduleWorkout>();
				for (int i = 0; i < request.WorkoutIds.Count; i++)
				{
					var workout = await _context.Set<Workout>().FindAsync(request.WorkoutIds[i]);
					if (workout == null)
					{
						await transaction.RollbackAsync();
						return new SheduleResult
						{
							Success = false,
							Message = $"Workout with ID {request.WorkoutIds[i]} not found."
						};
					}

					var scheduledWorkout = new SheduleWorkout
					{
						ScheduleId = schedule.ScheduleId,
						WorkoutId = request.WorkoutIds[i],
						SequenceOrder = i + 1,
						TrainerId = request.TrainerId,
						Completed = false
					};

					scheduledWorkouts.Add(scheduledWorkout);
				}

				_context.Set<SheduleWorkout>().AddRange(scheduledWorkouts);

				// Book trainers if specified
				var trainerBookingResult = await BookTrainersAsync(request.TrainerId,request.Date, request.StartTime, request.EndTime, request.MemberId);

				if (!trainerBookingResult.Success)
				{
						await transaction.RollbackAsync();
						return trainerBookingResult;
				}
				

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("Schedule created successfully for member {MemberId} on {Date}", request.MemberId, request.Date);

				return new SheduleResult
				{
					Success = true,
					Message = "Schedule created successfully.",
					ScheduleId = schedule.ScheduleId
				};
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error creating schedule for member {MemberId}", request.MemberId);
				return new SheduleResult
				{
					Success = false,
					Message = "Error creating schedule. Please try again."
				};
			}
		}

		/// <summary>
		/// Gets member's schedule details with workouts and trainers
		/// </summary>
		public async Task<MemberSheduleDetailDto> GetMemberScheduleDetailsAsync(int scheduleId, int memberId)
		{
			var schedule = await _context.Set<MemberShedule>()
				.Where(s => s.ScheduleId == scheduleId && s.MemberId == memberId)
				.Select(s => new MemberSheduleDetailDto
				{
					ScheduleId = s.ScheduleId,
					Date = s.Date,
					StartTime = s.StartTime,
					EndTime = s.EndTime,
					Status = s.Status,
					Notes = s.Notes,
					Workouts = _context.Set<SheduleWorkout>()
						.Where(sw => sw.ScheduleId == s.ScheduleId)
						.OrderBy(sw => sw.SequenceOrder)
						.Select(sw => new SheduleWorkoutDto
						{
							ScheduledWorkoutId = sw.ScheduledWorkoutId,
							WorkoutName = _context.Set<Workout>().Where(w => w.WorkoutId == sw.WorkoutId).Select(w => w.Name).FirstOrDefault(),
							WorkoutDescription = _context.Set<Workout>().Where(w => w.WorkoutId == sw.WorkoutId).Select(w => w.Description).FirstOrDefault(),
							WorkoutCategory = _context.Set<Workout>().Where(w => w.WorkoutId == sw.WorkoutId).Select(w => w.Category).FirstOrDefault(),
							DurationMinutes = _context.Set<Workout>().Where(w => w.WorkoutId == sw.WorkoutId).Select(w => w.DurationMinutes).FirstOrDefault(),
							SequenceOrder = sw.SequenceOrder,
							TrainerName = sw.TrainerId.HasValue ? _context.Set<Trainer>().Where(t => t.TrainerId == sw.TrainerId).Select(t => t.Name).FirstOrDefault() : null,
							TrainerId = sw.TrainerId,
							ActualStartTime = sw.ActualStartTime,
							ActualEndTime = sw.ActualEndTime,
							Completed = sw.Completed,
							Notes = sw.Notes
						}).ToList()
				})
				.FirstOrDefaultAsync();

			return schedule;
		}

		/// <summary>
		/// Gets all schedules for a member
		/// </summary>
		public async Task<List<MemberSheduleSummaryDto>> GetMemberSchedulesAsync(int memberId)
		{
			var query = _context.Set<MemberShedule>()
				.Where(s => s.MemberId == memberId);


			var schedules = await query
				.OrderBy(s => s.Date)
				.ThenBy(s => s.StartTime)
				.Select(s => new MemberSheduleSummaryDto
				{
					ScheduleId = s.ScheduleId,
					Date = s.Date,
					StartTime = s.StartTime,
					EndTime = s.EndTime,
					Status = s.Status,
					WorkoutCount = _context.Set<SheduleWorkout>().Count(sw => sw.ScheduleId == s.ScheduleId),
					TrainerAssigned = _context.Set<SheduleWorkout>().Any(sw => sw.ScheduleId == s.ScheduleId && sw.TrainerId.HasValue)
				})
				.ToListAsync();

			return schedules;
		}

		/// <summary>
		/// Gets all schedules assigned to a trainer
		/// </summary>
		public async Task<List<TrainerSheduleDto>> GetTrainerSchedulesAsync(int trainerId)
		{
			var query = from sw in _context.Set<SheduleWorkout>()
						join s in _context.Set<MemberShedule>() on sw.ScheduleId equals s.ScheduleId
						join w in _context.Set<Workout>() on sw.WorkoutId equals w.WorkoutId
						where sw.TrainerId == trainerId
						select new { sw, s, w };

			var schedules = await query
				.OrderBy(x => x.s.Date)
				.ThenBy(x => x.s.StartTime)
				.Select(x => new TrainerSheduleDto
				{
					ScheduleId = x.s.ScheduleId,
					ScheduledWorkoutId = x.sw.ScheduledWorkoutId,
					MemberId = x.s.MemberId,
					Date = x.s.Date,
					StartTime = x.s.StartTime,
					EndTime = x.s.EndTime,
					WorkoutName = x.w.Name,
					WorkoutCategory = x.w.Category,
					DurationMinutes = x.w.DurationMinutes,
					SequenceOrder = x.sw.SequenceOrder,
					Status = x.s.Status,
					Completed = x.sw.Completed,
					ActualStartTime = x.sw.ActualStartTime,
					ActualEndTime = x.sw.ActualEndTime,
					Notes = x.sw.Notes
				})
				.ToListAsync();

			return schedules;
		}

		
		/// <summary>
		/// Updates schedule status (completed, canceled, etc.)
		/// </summary>
		public async Task<SheduleResult> UpdateScheduleStatusAsync(int scheduleId, int memberId, string status, string notes = null)
		{
			var schedule = await _context.Set<MemberShedule>()
				.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.MemberId == memberId);

			if (schedule == null)
			{
				return new SheduleResult
				{
					Success = false,
					Message = "Schedule not found."
				};
			}

			schedule.Status = status;
			if (!string.IsNullOrEmpty(notes))
				schedule.Notes = notes;

			await _context.SaveChangesAsync();

			_logger.LogInformation("Schedule {ScheduleId} status updated to {Status}", scheduleId, status);

			return new SheduleResult
			{
				Success = true,
				Message = "Schedule status updated successfully."
			};
		}

		/// <summary>
		/// Gets all available workouts with filtering options
		/// </summary>
		public async Task<List<Workout>> GetWorkoutsAsync(string category = null, string difficulty = null, int? maxDuration = null)
		{
			var query = _context.Set<Workout>().AsQueryable();

			if (!string.IsNullOrEmpty(category))
				query = query.Where(w => w.Category.ToLower() == category.ToLower());

			if (!string.IsNullOrEmpty(difficulty))
				query = query.Where(w => w.Difficulty.ToLower() == difficulty.ToLower());

			if (maxDuration.HasValue)
				query = query.Where(w => w.DurationMinutes <= maxDuration.Value);

			var workouts = await query
				.OrderBy(w => w.Category)
				.ThenBy(w => w.Name)
				.Select(w => new Workout
				{
					WorkoutId = w.WorkoutId,
					Name = w.Name,
					Description = w.Description,
					Category = w.Category,
					DurationMinutes = w.DurationMinutes,
					Difficulty = w.Difficulty,
					EquipmentNeeded = w.EquipmentNeeded
				})
				.ToListAsync();

			return workouts;
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
						.Count(sw => sw.TrainerId == t.TrainerId && sw.Status == "Completed")
				})
				.ToListAsync();

			return trainers;
		}


		private async Task<bool> IsTimeSlotAvailableAsync(int memberId, DateTime date, TimeSpan startTime, TimeSpan endTime)
		{
			var conflictingSchedule = await _context.Set<MemberShedule>()
				.AnyAsync(s => s.MemberId == memberId &&
							  s.Date == date &&
							  s.Status != "canceled" &&
							  ((s.StartTime <= startTime && s.EndTime > startTime) ||
							   (s.StartTime < endTime && s.EndTime >= endTime) ||
							   (s.StartTime >= startTime && s.EndTime <= endTime)));

			return !conflictingSchedule;
		}

		private async Task<SheduleResult> BookTrainersAsync(int trainerId, DateTime date, TimeSpan startTime, TimeSpan endTime, int memberId)
		{
			
			var availability = await _context.Set<TrainerAvailability>()
					.FirstOrDefaultAsync(ta => ta.TrainerId == trainerId &&
											  ta.Date == date &&
											  ta.StartTime <= startTime &&
											  ta.EndTime >= endTime &&
											  ta.IsAvailable);

			if (availability == null)
			{
				return new SheduleResult
				{
					Success = false,
					Message = $"Trainer {trainerId} is not available for the selected time slot.select different Trainer"
				};
			}

			availability.IsAvailable = false;
			availability.BookedBy = memberId;
		

			return new SheduleResult { Success = true };
		}


	}

}
