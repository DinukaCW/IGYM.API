using IGYM.Interface;
using IGYM.Interface.GymModule;
using IGYM.Interface.UserModule;
using IGYM.Model.SheduleModule.DTOs;
using IGYM.Model.SheduleModule.Entities;
using IGYM.Model.UserModule.DTOs;
using IGYM.Service;
using IGYM.Service.GymModule;
using IGYM.Service.UserModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace IGYM.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GymController : ControllerBase
	{
		private readonly IConfiguration _configuration; // Configuration for application settings
		private readonly IGymSheduleService _gymSheduleService; // Service for managing gym schedules

		public GymController(IConfiguration configuration, IGymSheduleService gymSheduleService)
		{
			_configuration = configuration;
			_gymSheduleService = gymSheduleService;
		}


		[HttpPost("create-request")]
		public async Task<IActionResult> CreateScheduleRequest([FromBody] CreateSheduleRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var requestId = await _gymSheduleService.CreateScheduleRequestAsync(request);
				return Ok(new { RequestId = requestId });
			}
			catch (ArgumentNullException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (DbUpdateException ex)
			{
				return StatusCode(500, new { message = "Error saving schedule request" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpGet("trainer/pending-requests/{trainerId}")]
		public async Task<IActionResult> GetTrainerPendingRequests(int trainerId)
		{
			try
			{
				var requests = await _gymSheduleService.GetTrainerPendingRequestsAsync(trainerId);
				return Ok(requests);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpPost("create-workout-plan")]
		public async Task<IActionResult> CreateWorkoutPlan([FromBody] CreateWorkoutPlanDto planDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var success = await _gymSheduleService.CreateWorkoutPlanAsync(planDto);
				return success ? Ok() : BadRequest(new { message = "Failed to create workout plan" });
			}
			catch (ArgumentNullException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpGet("member/workout-plans/{memberId}")]
		public async Task<IActionResult> GetMemberWorkoutPlans(int memberId)
		{
			try
			{
				var plans = await _gymSheduleService.GetMemberWorkoutPlansAsync(memberId);
				return Ok(plans);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpGet("member/workout-plan")]
		public async Task<IActionResult> GetMemberWorkoutPlanById([FromQuery] int memberId, [FromQuery] int scheduleId)
		{
			try
			{
				var plan = await _gymSheduleService.GetMemberWorkoutPlanByIdAsync(memberId, scheduleId);
				return plan == null ? NotFound() : Ok(plan);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpGet("daily-workouts")]
		public async Task<IActionResult> GetDailyWorkouts([FromQuery] int memberId, [FromQuery] int scheduleId, [FromQuery] int dayNumber)
		{
			try
			{
				var workouts = await _gymSheduleService.GetDailyWorkoutsAsync(memberId, scheduleId, dayNumber);
				return Ok(workouts);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{ 
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpPut("update-workout-completion")]
		public async Task<IActionResult> UpdateWorkoutCompletion([FromQuery] int memberId, [FromBody] List<WorkOutCompletionDto> completions)
		{
			try
			{
				var success = await _gymSheduleService.UpdateWorkoutCompletionAsync(memberId, completions);
				return success ? Ok() : BadRequest(new { message = "Failed to update workout completion" });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpPut("update-schedule-status/{scheduleId}")]
		public async Task<IActionResult> UpdateScheduleStatus(int scheduleId, [FromQuery] PlanStatus status)
		{
			try
			{
				var success = await _gymSheduleService.UpdateScheduleStatusAsync(scheduleId, status);
				return success ? Ok() : NotFound(new { message = "Schedule not found" });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpPut("update-request-status/{requestId}")]
		public async Task<IActionResult> UpdateRequestStatus(int requestId, [FromQuery] string status)
		{
			try
			{
				var success = await _gymSheduleService.UpdateRequestStatusAsync(requestId, status);
				return success ? Ok() : NotFound(new { message = "Request not found" });
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpGet("available-workouts")]
		public async Task<IActionResult> GetAvailableWorkouts()
		{
			try
			{
				var workouts = await _gymSheduleService.GetAvailableWorkoutsAsync();
				return Ok(workouts);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

		[HttpGet("trainers")]
		public async Task<IActionResult> GetTrainers([FromQuery] bool activeOnly = true, [FromQuery] string specialization = null)
		{
			try
			{
				var trainers = await _gymSheduleService.GetTrainersAsync(activeOnly, specialization);
				return Ok(trainers);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error" });
			}
		}

	}
}
