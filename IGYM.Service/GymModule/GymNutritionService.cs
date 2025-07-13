using Google;
using IGYM.Interface.GymModule;
using IGYM.Model;
using IGYM.Model.NutritionModule.DTOs;
using IGYM.Model.NutritionModule.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Service.GymModule
{
	public class GymNutritionService : IGymNutritionService
	{
		private readonly IGYMDbContext _context;
		private readonly ILogger<GymNutritionService> _logger;

		public GymNutritionService(IGYMDbContext context, ILogger<GymNutritionService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<NutritionPlanRequest> CreateNutritionPlanRequestAsync(CreateNutritionPlanRequestDto requestDto)
		{
			try
			{
				// Validate input
				if (requestDto == null)
				{
					throw new ArgumentNullException(nameof(requestDto), "Request data cannot be null");
				}

				// Validate member exists
				var memberExists = await _context.Users.AnyAsync(u => u.UserID == requestDto.MemberId);
				if (!memberExists)
				{
					throw new ArgumentException("Member not found", nameof(requestDto.MemberId));
				}

				// Validate trainer exists
				var trainerExists = await _context.Trainer.AnyAsync(t => t.TrainerId == requestDto.TrainerId);
				if (!trainerExists)
				{
					throw new ArgumentException("Trainer not found", nameof(requestDto.TrainerId));
				}

				// Validate weight and height
				if (requestDto.Weight <= 0)
				{
					throw new ArgumentException("Weight must be positive", nameof(requestDto.Weight));
				}

				if (requestDto.Height <= 0)
				{
					throw new ArgumentException("Height must be positive", nameof(requestDto.Height));
				}

				var request = new NutritionPlanRequest
				{
					MemberId = requestDto.MemberId,
					TrainerId = requestDto.TrainerId,
					Goal = requestDto.Goal,
					Weight = requestDto.Weight,
					Height = requestDto.Height,
					DietPreference = requestDto.DietPreference,
					MedicalNotes = requestDto.MedicalNotes,
					AdditionalNotes = requestDto.AdditionalNotes
				};

				_context.NutritionPlanRequest.Add(request);
				await _context.SaveChangesAsync();

				return request;
			}
			catch (DbUpdateException ex)
			{
				_logger.LogError(ex, "Database error while creating nutrition plan request");
				throw new ApplicationException("Failed to save nutrition plan request", ex);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating nutrition plan request");
				throw;
			}
		}

		public async Task<List<NutritionPlanRequestDto>> GetPendingNutritionPlanRequestsAsync(int trainerId)
		{
			try
			{
				// Validate trainer exists
				var trainerExists = await _context.Trainer.AnyAsync(t => t.TrainerId == trainerId);
				if (!trainerExists)
				{
					throw new ArgumentException("Trainer not found", nameof(trainerId));
				}

				return await _context.NutritionPlanRequest
					.Where(r => r.TrainerId == trainerId && r.Status == NutritionPlanRequestStatus.Pending)
					.Include(r => r.Member)
					.Select(r => new NutritionPlanRequestDto
					{
						Id = r.Id,
						MemberName = r.Member.Name,
						Goal = r.Goal,
						Weight = r.Weight,
						Height = r.Height,
						DietPreference = r.DietPreference,
						RequestDate = r.RequestDate,
						Status = r.Status
					})
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving pending nutrition plan requests for trainer {TrainerId}", trainerId);
				throw;
			}
		}

		public async Task<NutritionPlan> CreateNutritionPlanAsync(CreateNutritionPlanDto planDto)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// Validate input
				if (planDto == null)
				{
					throw new ArgumentNullException(nameof(planDto), "Plan data cannot be null");
				}

				// Verify the request exists and is pending
				var request = await _context.NutritionPlanRequest
					.FirstOrDefaultAsync(r => r.Id == planDto.RequestId);

				if (request == null)
				{
					throw new ArgumentException("Nutrition plan request not found", nameof(planDto.RequestId));
				}

				if (request.Status != NutritionPlanRequestStatus.Pending)
				{
					throw new InvalidOperationException($"Cannot create plan for request with status {request.Status}");
				}

				// Validate meal plans
				if (planDto.MealPlans == null || !planDto.MealPlans.Any())
				{
					throw new ArgumentException("At least one meal plan is required", nameof(planDto.MealPlans));
				}

				// Create the nutrition plan
				var plan = new NutritionPlan
				{
					RequestId = planDto.RequestId,
					TrainerNotes = planDto.TrainerNotes
				};

				_context.NutritionPlan.Add(plan);
				await _context.SaveChangesAsync();

				// Add meal plans
				foreach (var mealPlanDto in planDto.MealPlans)
				{
					if (mealPlanDto.MealItems == null || !mealPlanDto.MealItems.Any())
					{
						throw new ArgumentException("Each meal plan must contain at least one meal item", nameof(mealPlanDto.MealItems));
					}

					var mealPlan = new MealPlan
					{
						NutritionPlanId = plan.Id,
						MealType = mealPlanDto.MealType
					};

					_context.MealPlan.Add(mealPlan);
					await _context.SaveChangesAsync();

					// Add meal items
					foreach (var mealItemDto in mealPlanDto.MealItems)
					{
						// Validate food item exists
						var foodItemExists = await _context.FoodItem.AnyAsync(f => f.Id == mealItemDto.FoodItemId);
						if (!foodItemExists)
						{
							throw new ArgumentException($"Food item with ID {mealItemDto.FoodItemId} not found", nameof(mealItemDto.FoodItemId));
						}

						if (mealItemDto.Quantity <= 0)
						{
							throw new ArgumentException("Quantity must be positive", nameof(mealItemDto.Quantity));
						}

						var mealItem = new MealItem
						{
							MealPlanId = mealPlan.Id,
							FoodItemId = mealItemDto.FoodItemId,
							Quantity = mealItemDto.Quantity,
							PreparationNotes = mealItemDto.PreparationNotes
						};

						_context.MealItem.Add(mealItem);
					}
				}

				// Update request status
				request.Status = NutritionPlanRequestStatus.Completed;
				_context.NutritionPlanRequest.Update(request);

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return plan;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error creating nutrition plan");
				throw;
			}
		}

		public async Task<NutritionPlanDto> GetNutritionPlanAsync(int planId)
		{
			try
			{
				if (planId <= 0)
				{
					throw new ArgumentException("Invalid plan ID", nameof(planId));
				}

				var plan = await _context.NutritionPlan
					.Include(p => p.Request)
						.ThenInclude(r => r.Member)
					.Include(p => p.MealPlans)
						.ThenInclude(mp => mp.MealItems)
							.ThenInclude(mi => mi.FoodItem)
					.FirstOrDefaultAsync(p => p.Id == planId);

				if (plan == null)
				{
					throw new KeyNotFoundException($"Nutrition plan with ID {planId} not found");
				}

				return new NutritionPlanDto
				{
					Id = plan.Id,
					RequestId = plan.RequestId,
					CreatedDate = plan.CreatedDate,
					TrainerNotes = plan.TrainerNotes,
					MemberName = plan.Request.Member.Name,
					Goal = plan.Request.Goal,
					DietPreference = plan.Request.DietPreference,
					MealPlans = plan.MealPlans.Select(mp => new MealPlanDto
					{
						MealType = mp.MealType,
						MealItems = mp.MealItems.Select(mi => new MealItemDto
						{
							FoodItemId = mi.FoodItemId,
							FoodName = mi.FoodItem.Name,
							Quantity = mi.Quantity,
							Calories = mi.FoodItem.Calories * (mi.Quantity / 100),
							Protein = mi.FoodItem.Protein * (mi.Quantity / 100),
							Carbs = mi.FoodItem.Carbs * (mi.Quantity / 100),
							Fats = mi.FoodItem.Fats * (mi.Quantity / 100),
							PreparationNotes = mi.PreparationNotes
						}).ToList()
					}).ToList()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving nutrition plan {PlanId}", planId);
				throw;
			}
		}

		public async Task<List<NutritionPlanDto>> GetMemberNutritionPlansAsync(int memberId)
		{
			try
			{
				if (memberId <= 0)
				{
					throw new ArgumentException("Invalid member ID", nameof(memberId));
				}

				// Validate member exists
				var memberExists = await _context.Users.AnyAsync(u => u.UserID == memberId);
				if (!memberExists)
				{
					throw new ArgumentException("Member not found", nameof(memberId));
				}

				return await _context.NutritionPlan
					.Include(p => p.Request)
					.Include(p => p.MealPlans)
						.ThenInclude(mp => mp.MealItems)
							.ThenInclude(mi => mi.FoodItem)
					.Where(p => p.Request.MemberId == memberId)
					.Select(p => new NutritionPlanDto
					{
						Id = p.Id,
						RequestId = p.RequestId,
						CreatedDate = p.CreatedDate,
						Goal = p.Request.Goal,
						MealPlans = p.MealPlans.Select(mp => new MealPlanDto
						{
							MealType = mp.MealType,
							MealItems = mp.MealItems.Select(mi => new MealItemDto
							{
								FoodName = mi.FoodItem.Name,
								Quantity = mi.Quantity,
								Calories = mi.FoodItem.Calories * (mi.Quantity / 100)
							}).ToList()
						}).ToList()
					})
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving nutrition plans for member {MemberId}", memberId);
				throw;
			}
		}

		public async Task<bool> UpdateRequestStatusAsync(int requestId, NutritionPlanRequestStatus status)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				if (requestId <= 0)
				{
					throw new ArgumentException("Invalid request ID", nameof(requestId));
				}

				var request = await _context.NutritionPlanRequest.FindAsync(requestId);
				if (request == null)
				{
					return false;
				}

				// Validate status transition
				if (request.Status == NutritionPlanRequestStatus.Completed &&
					status != NutritionPlanRequestStatus.Completed)
				{
					throw new InvalidOperationException("Cannot modify status of a completed request");
				}

				request.Status = status;
				_context.NutritionPlanRequest.Update(request);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return true;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error updating status for request {RequestId}", requestId);
				throw;
			}
		}
	}
}
