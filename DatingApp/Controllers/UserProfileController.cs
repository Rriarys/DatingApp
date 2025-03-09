using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatingApp.Dto;
using DatingApp.Services;

namespace DatingApp.Controllers
{
    [Authorize]
    [Route("api/profile")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly UserService _userService;

        public UserProfileController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null)
                return NotFound("User profile not found");

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            var success = await _userService.UpdateUserProfileAsync(userId, dto);

            if (!success)
                return BadRequest("Failed to update profile");

            return Ok("Profile updated successfully");
        }


        [HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto([FromBody] UserPhotoDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");

            // Заглушка: просто выводим информацию о полученном фото
            if (dto == null)
                return BadRequest("No photo data provided.");

            // Эмуляция успешного обновления фото
            var success = true; // Симуляция успеха

            if (!success)
                return BadRequest("Failed to update profile photo");

            return Ok("Profile photo updated successfully. (Simulated response)");
        }

        /*[HttpPost("photo")]
        public async Task<IActionResult> UploadPhoto([FromBody] UserPhotoDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");
            var success = await _userService.UpdateUserPhotoAsync(userId, dto);
            if (!success)
                return BadRequest("Failed to update profile photo");

            return Ok("Profile photo updated successfully");
        }*/
    }
}
