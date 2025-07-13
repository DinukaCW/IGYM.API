using IGYM.Model.AdminModule;
using IGYM.Model.SheduleModule.DTOs;
using IGYM.Model.SheduleModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface.GymModule
{
	public interface IGymSheduleService
	{

		Task<int> CreateScheduleRequestAsync(CreateSheduleRequest request);
		Task<List<TrainerSheduleRequestDto>> GetTrainerPendingRequestsAsync(int trainerId);
		Task<bool> CreateWorkoutPlanAsync(CreateWorkoutPlanDto planDto);
		Task<List<MemberWorkoutPlanDto>> GetMemberWorkoutPlansAsync(int memberId);
		Task<MemberWorkoutPlanDto> GetMemberWorkoutPlanByIdAsync(int memberId, int scheduleId);
		Task<List<WorkoutDetailDto>> GetDailyWorkoutsAsync(int memberId, int scheduleId, int dayNumber);
		Task<bool> UpdateWorkoutCompletionAsync(int memberId, List<WorkOutCompletionDto> completions);
		Task<bool> UpdateScheduleStatusAsync(int scheduleId, PlanStatus status);
		Task<bool> UpdateRequestStatusAsync(int requestId, string status);
		Task<List<WorkoutResponseDto>> GetAvailableWorkoutsAsync();
		Task<List<TrainerDto>> GetTrainersAsync(bool activeOnly = true, string specialization = null);
	}
}
