using DatingApp.Services;
using Microsoft.AspNetCore.Mvc;
using DatingApp.Dto;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
        {
            var result = await _userService.RegisterUserAsync(dto);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Registration success");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var tokens = await _userService.AuthenticateUserAsync(dto);
            if (tokens == null)
                return Unauthorized("Wrong email or password");

            // Возвращаем токены в ответе
            return Ok(new { AccessToken = tokens.Value.AccessToken, RefreshToken = tokens.Value.RefreshToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshDto dto)
        {
            var tokens = await _userService.RefreshAccessTokenAsync(dto);
            if (tokens == null)
                return Unauthorized("Invalid or expired refresh token");

            SetAuthCookies(tokens.Value.AccessToken, tokens.Value.RefreshToken);
            return Ok(new { tokens.Value.AccessToken, tokens.Value.RefreshToken });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("DatingApp_AuthToken");
            Response.Cookies.Delete("DatingApp_RefreshToken");
            return Ok("Logout success");
        }

        [Authorize]
        [HttpGet("secure-endpoint")]
        public IActionResult GetSecureData()
        {
            return Ok("Access to secure resource is allowed");
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append("DatingApp_AuthToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            Response.Cookies.Append("DatingApp_RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}