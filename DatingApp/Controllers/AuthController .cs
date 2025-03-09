using DatingApp.Services;
using Microsoft.AspNetCore.Mvc;
using DatingApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Data;

namespace DatingApp.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Registration([FromBody] User user)
        {
            var result = await _userService.RegisterUserAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Registration success");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var tokens = await _userService.AuthenticateUserAsync(user);

            if (tokens == null)
            {
                return Unauthorized("Wrong email or password");
            }

            SetAuthCookies(tokens.Value.AccessToken, tokens.Value.RefreshToken);

            return Ok(tokens);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
        {
            var tokens = await _userService.RefreshAccessTokenAsync(request.RefreshToken);

            if (tokens == null)
            {
                return Unauthorized("Invalid or expired refresh token");
            }

            SetAuthCookies(tokens.Value.AccessToken, tokens.Value.RefreshToken);
            return Ok(tokens);
        }

        [Authorize]
        [HttpGet("secure-endpoint")]
        public IActionResult GetSecureData()
        {
            return Ok("Access to secure resource is allowed");
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");

            return Ok("Logout success");
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append("AuthToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            }); 

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
