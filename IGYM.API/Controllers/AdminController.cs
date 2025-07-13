using IGYM.Interface.GymModule;
using IGYM.Model.AdminModule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IGYM.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AdminController : ControllerBase
	{
		private readonly IConfiguration _configuration; // Configuration for application settings
		private readonly IAdminService _adminService; // Service for managing gym schedules

		public AdminController(IConfiguration configuration, IAdminService adminService)
		{
			_configuration = configuration;
			_adminService = adminService;
		}

		[HttpPost("workouts")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> CreateWorkout([FromForm] AddWorkoutDto workoutDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Image file type validation
				if (workoutDto.ImageFile != null)
				{
					var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
					var extension = Path.GetExtension(workoutDto.ImageFile.FileName).ToLowerInvariant();

					if (!allowedExtensions.Contains(extension))
					{
						return BadRequest(new { message = "Only JPG, JPEG, and PNG images are allowed." });
					}
				}

				var result = await _adminService.AddWorkoutAsync(workoutDto);

				return CreatedAtAction(nameof(CreateWorkout), new { id = result.WorkoutId }, result);
			}
			catch (Exception ex)
			{
				// Optional: log error here
				return StatusCode(500, new { message = "Internal server error", error = ex.Message });
			}
		}


		[HttpPost("add-trainer")]
		public async Task<IActionResult> CreateTrainer([FromBody] AddTrainerDto request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return ValidationProblem(ModelState);
				}

				var passwordValidationResult = ValidatePassword(request.Password);
				if (!passwordValidationResult.IsValid)
				{
					return BadRequest(new { message = passwordValidationResult.ErrorMessage });
				}

				var (trainerId, userId) = await _adminService.AddTrainerAsync(request);

				return Created("", new
				{
					TrainerId = trainerId,
					UserId = userId,
					Message = "Trainer account created successfully. Verification email sent."
				});
			}
			catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
			{
				return Conflict(new { message = "Username or email already exists." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error", error = ex.Message });
			}
		}


		// Supporting methods and classes
		private PasswordValidationResult ValidatePassword(string password)
		{
			if (password.Length < 8)
				return new PasswordValidationResult(false, "Password must be at least 8 characters");

			if (!password.Any(char.IsUpper))
				return new PasswordValidationResult(false, "Password must contain at least one uppercase letter");

			if (!password.Any(char.IsDigit))
				return new PasswordValidationResult(false, "Password must contain at least one number");

			return new PasswordValidationResult(true);
		}


		[HttpDelete("trainer")]
		public async Task<IActionResult> DeleteTrainer(int trainerId)
		{
			var result = await _adminService.DeleteTrainerAsync(trainerId);
			if (result.Success)
				return Ok(result);
			return BadRequest(result);
		}

		/// <summary>
		/// Deletes a workout by ID.
		/// </summary>
		[HttpDelete("workout")]
		public async Task<IActionResult> DeleteWorkout(int workoutId)
		{
			var result = await _adminService.DeleteWorkoutAsync(workoutId);
			if (result.Success)
				return Ok(result);
			return BadRequest(result);
		}

		[HttpPost("food-items")]
		public async Task<IActionResult> AddFoodItem([FromBody] CreateFoodItemDto foodItemDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return ValidationProblem(ModelState);
				}

				var result = await _adminService.AddFoodItemAsync(foodItemDto);

				return Ok (result);

			}
			
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Internal server error", error = ex.Message });
			}
		}
		[HttpDelete("foodItem")]
		public async Task<IActionResult> DeleteFoodItem(int foodItemId)
		{
			var result = await _adminService.DeleteFoodItemAsync(foodItemId);
			if (result.Success)
				return Ok(result);
			return BadRequest(result);
		}
		public record PasswordValidationResult(bool IsValid, string? ErrorMessage = null);



	}
}