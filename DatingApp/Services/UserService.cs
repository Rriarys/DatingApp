using DatingApp.Data;
using Microsoft.AspNetCore.Identity;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DatingApp.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterUserAsync(User user)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<(string AccessToken, string RefreshToken)?> AuthenticateUserAsync(User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (existingUser == null || _passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, user.PasswordHash) != PasswordVerificationResult.Success)
            {
                return null;
            }

            var accessToken = GenerateAccessToken(existingUser);
            var refreshToken = await GenerateRefreshTokenAsync(existingUser);
            return (accessToken, refreshToken);
        }

        private string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                new[] { new Claim(ClaimTypes.Email, user.Email) },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken.Token;
        }

        public async Task<(string AccessToken, string RefreshToken)?> RefreshAccessTokenAsync(string refreshToken)
        {
            var existingToken = await _context.RefreshTokens.Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (existingToken == null || existingToken.Expires < DateTime.UtcNow || existingToken.IsRevoked)
            {
                return null;
            }

            existingToken.IsRevoked = true;
            var newAccessToken = GenerateAccessToken(existingToken.User);
            var newRefreshToken = await GenerateRefreshTokenAsync(existingToken.User);

            await _context.SaveChangesAsync();
            return (newAccessToken, newRefreshToken);
        }
    }
}
