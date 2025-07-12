using IGYM.Model.SheduleModule.DTOs;
using IGYM.Model.SheduleModule.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Interface.SheduleModule
{
	public interface IGymSheduleService
	{
		Task<SheduleResult> CreateMemberScheduleAsync(CreateSheduleRequest request);
		Task<MemberSheduleDetailDto> GetMemberScheduleDetailsAsync(int scheduleId, int memberId);
		Task<List<MemberSheduleSummaryDto>> GetMemberSchedulesAsync(int memberId);
		Task<List<TrainerSheduleDto>> GetTrainerSchedulesAsync(int trainerId);
		Task<SheduleResult> UpdateScheduleStatusAsync(int scheduleId, int memberId, string status, string notes = null);
		Task<List<Workout>> GetWorkoutsAsync(string category = null, string difficulty = null, int? maxDuration = null);
		Task<List<TrainerDto>> GetTrainersAsync(bool activeOnly = true, string specialization = null);

	}
}
