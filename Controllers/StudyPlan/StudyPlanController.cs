// File: Controllers/StudyPlanController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sciencetopia.Models;
using Sciencetopia.Services;
using System.Security.Claims;

namespace Sciencetopia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Ensure only authenticated users can access
    public class StudyPlanController : ControllerBase
    {
        private readonly StudyPlanService _studyPlanService;

        public StudyPlanController(StudyPlanService studyPlanService)
        {
            _studyPlanService = studyPlanService;
        }

        [HttpPost("SaveStudyPlan")]
        public async Task<IActionResult> SaveStudyPlan([FromBody] StudyPlanDTO studyPlanDTO)
        {
            // Retrieve the user's ID from the ClaimsPrincipal
            string userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // Ensure the user is authenticated
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var result = await _studyPlanService.SaveStudyPlanAsync(studyPlanDTO, userId);
            if (result)
            {
                return Ok(); // Plan saved successfully
            }
            else
            {
                return BadRequest("该学习计划已经存在。");
            }
        }

        [HttpGet("FetchStudyPlans")]
        public async Task<IActionResult> FetchStudyPlans([FromQuery] string? targetUserId = null)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User is not authenticated.");
            }

            targetUserId ??= currentUserId;

            var studyPlans = await _studyPlanService.GetStudyPlansByUserIdAsync(currentUserId, targetUserId);

            if (studyPlans == null || !studyPlans.Any())
            {
                return Ok(new List<StudyPlanDTO>());  // Return an empty list if no study plans are found
            }

            return Ok(studyPlans);
        }

        [HttpPost("UpdateStudyPlan")]
        public async Task<IActionResult> UpdateStudyPlan([FromBody] StudyPlanDTO studyPlanDTO)
        {
            string userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // Ensure the user is authenticated
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var success = await _studyPlanService.UpdateStudyPlanAsync(studyPlanDTO);
            if (success)
            {
                return Ok(new { message = "Study plan updated successfully." });
            }
            else
            {
                return NotFound(new { message = "Study plan not found or could not be updated." });
            }
        }

        [HttpPost("MarkStudyPlanAsCompleted")]
        public async Task<IActionResult> MarkStudyPlanAsCompleted(string studyPlanTitle)
        {
            string userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // Ensure the user is authenticated
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var success = await _studyPlanService.MarkStudyPlanAsCompletedAsync(studyPlanTitle, userId);
            if (success)
            {
                return Ok(new { message = "Study plan marked as completed." });
            }
            else
            {
                return NotFound(new { message = "Study plan not found or could not be marked as completed." });
            }
        }

        [HttpDelete("DisMarkStudyPlanAsCompleted")]
        public async Task<IActionResult> DisMarkStudyPlanAsCompleted(string studyPlanTitle)
        {
            string userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // Ensure the user is authenticated
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var success = await _studyPlanService.DisMarkStudyPlanAsCompletedAsync(studyPlanTitle, userId);
            if (success)
            {
                return Ok(new { message = "Removed marking study plan as completed." });
            }
            else
            {
                return NotFound(new { message = "Study plan not found or mark of completion could not be removed." });
            }
        }

        [HttpGet("CountCompletedStudyPlansByUserId")]
        public async Task<IActionResult> CountCompletedStudyPlans(string userId)
        {
            // Ensure the user is authenticated
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var count = await _studyPlanService.CountCompletedStudyPlansAsync(userId);

            return Ok(count);
        }

        [HttpDelete("DeleteStudyPlan")]
        public async Task<IActionResult> DeleteStudyPlan(string studyPlanTitle)
        {
            // Fetch the current authenticated user's ID from claims
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            var success = await _studyPlanService.DeleteStudyPlanAsync(studyPlanTitle, currentUserId);
            if (success)
            {
                return Ok(new { message = "Study plan deleted successfully." });
            }
            else
            {
                return NotFound(new { message = "Study plan not found or could not be deleted." });
            }
        }

        [HttpPost("SetStudyPlanPrivacy")]
        public async Task<IActionResult> SetStudyPlanPrivacy([FromBody] SetPrivacyRequest request)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ensure the user is authenticated
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            // Call the service method to update the privacy setting
            var success = await _studyPlanService.SetStudyPlanPrivacyAsync(userId, request.StudyPlanId, request.Privacy);

            if (!success)
            {
                return NotFound("Study plan not found or could not be updated.");
            }

            return Ok("Privacy updated successfully.");
        }
    }
}
