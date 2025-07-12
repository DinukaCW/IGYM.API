using IGYM.Interface;
using IGYM.Interface.SheduleModule;
using IGYM.Interface.UserModule;
using IGYM.Model.SheduleModule.DTOs;
using IGYM.Model.UserModule.DTOs;
using IGYM.Service;
using IGYM.Service.UserModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

	
		[HttpPost("Create-Shedule")]
		public async Task<IActionResult> CreateMemeberShedule([FromBody] CreateSheduleRequest sheduleRequest)
		{
			try
			{
				// Validate the model state
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Submit the order to the business service
				var result = await _gymSheduleService.CreateMemberScheduleAsync(sheduleRequest);

				if (!result.Success)
				{
					return BadRequest(new { message = result.Message });
				}
				return Ok(new { message = result.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Internal server error:{ex.InnerException}");
			}
		}

		[HttpGet("get-schedule-details")]
		public async Task<IActionResult> GetMemberScheduleDetails([FromQuery] int scheduleId, [FromQuery] int memberId)
		{
			try
			{
				var scheduleDetails = await _gymSheduleService.GetMemberScheduleDetailsAsync(scheduleId, memberId);

				if (scheduleDetails == null)
				{
					return NotFound(new { message = "Schedule not found" });
				}

				return Ok(scheduleDetails);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

		[HttpGet("get-member-schedules")]
		public async Task<IActionResult> GetMemberSchedules([FromQuery] int memberId)
		{
			try
			{
				var schedules = await _gymSheduleService.GetMemberSchedulesAsync(memberId);
				return Ok(schedules);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

		[HttpGet("get-trainer-schedules")]
		public async Task<IActionResult> GetTrainerSchedules([FromQuery] int trainerId)
		{
			try
			{
				var schedules = await _gymSheduleService.GetTrainerSchedulesAsync(trainerId);
				return Ok(schedules);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

		[HttpPatch("update-schedule-status")]
		public async Task<IActionResult> UpdateScheduleStatus(
			[FromQuery] int scheduleId,
			[FromQuery] int memberId,
			[FromQuery] string status,
			[FromQuery] string notes = null)
		{
			try
			{
				var result = await _gymSheduleService.UpdateScheduleStatusAsync(scheduleId, memberId, status, notes);

				if (!result.Success)
				{
					return BadRequest(new { message = result.Message });
				}

				return Ok(new { message = result.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

		[HttpGet("get-workouts")]
		public async Task<IActionResult> GetWorkouts(
			[FromQuery] string category = null,
			[FromQuery] string difficulty = null,
			[FromQuery] int? maxDuration = null)
		{
			try
			{
				var workouts = await _gymSheduleService.GetWorkoutsAsync(category, difficulty, maxDuration);
				return Ok(workouts);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

		[HttpGet("get-trainers")]
		public async Task<IActionResult> GetTrainers(
			[FromQuery] bool activeOnly = true,
			[FromQuery] string specialization = null)
		{
			try
			{
				var trainers = await _gymSheduleService.GetTrainersAsync(activeOnly, specialization);
				return Ok(trainers);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error: {ex.Message}");
			}
		}

	}
}
