using IGYM.Model.NutritionModule.DTOs;
using IGYM.Model.NutritionModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface.GymModule
{
	public interface IGymNutritionService
	{
		Task<NutritionPlanRequest> CreateNutritionPlanRequestAsync(CreateNutritionPlanRequestDto request);
		Task<List<NutritionPlanRequestDto>> GetPendingNutritionPlanRequestsAsync(int trainerId);
		Task<NutritionPlan> CreateNutritionPlanAsync(CreateNutritionPlanDto planDto);
		Task<NutritionPlanDto> GetNutritionPlanAsync(int planId);
		Task<List<NutritionPlanDto>> GetMemberNutritionPlansAsync(int memberId);
		Task<bool> UpdateRequestStatusAsync(int requestId, NutritionPlanRequestStatus status);
	}
}
