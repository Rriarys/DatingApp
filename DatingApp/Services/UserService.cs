using DatingApp.Data;
using DatingApp.Dto;
using DatingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        // Регистрация пользователя
        public async Task<IdentityResult> RegisterUserAsync(UserRegistrationDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(null, dto.PasswordHash)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

       // Аутентификация и получение токенов
        public async Task<(string AccessToken, string RefreshToken)?> AuthenticateUserAsync(UserLoginDto dto)
        {
            // Проверка пользователя
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.PasswordHash) != PasswordVerificationResult.Success)
            {
                return null;
            }

            // Генерация токенов
            var accessToken = GenerateAccessToken(user);  // Генерация access-токена
            var refreshToken = await GenerateRefreshTokenAsync(user);  // Генерация refresh-токена

            // Возвращаем оба токена
            return (accessToken, refreshToken);
        }

        // Достать из бд
        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.AboutUser)
                .Include(u => u.UserLocation)
                .Include(u => u.Preferences)
                .Include(u => u.ProfilePhoto)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            // Преобразуем модель пользователя в DTO
            var profileDto = new UserProfileDto
            {
                FirstName = user.AboutUser.FirstName,
                BirthDate = user.AboutUser.BirthDate,
                About = user.AboutUser.About,
                Latitude = user.UserLocation.Latitude,
                Longitude = user.UserLocation.Longitude,
                PreferredGender = user.Preferences.PreferredGender,
                MinAge = user.Preferences.MinAge,
                MaxAge = user.Preferences.MaxAge,
                MaxDistanceKm = (int)user.Preferences.MaxDistanceKm
            };

            return profileDto;
        }

        // Обновление информации о пользователе
        public async Task<bool> UpdateUserProfileAsync(int userId, UserProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.AboutUser)
                .Include(u => u.Preferences)
                .Include(u => u.UserLocation)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                // Если пользователь не найден, возвращаем false
                return false;
            }

            // Обновляем информацию о пользователе
            if (user.AboutUser != null)
            {
                user.AboutUser.FirstName = dto.FirstName;
                user.AboutUser.BirthDate = dto.BirthDate;
                user.AboutUser.About = dto.About;
            }

            // Обновляем местоположение
            if (user.UserLocation != null)
            {
                user.UserLocation.Latitude = dto.Latitude;
                user.UserLocation.Longitude = dto.Longitude;
            }

            // Обновляем предпочтения
            if (user.Preferences != null)
            {
                user.Preferences.MinAge = dto.MinAge;
                user.Preferences.MaxAge = dto.MaxAge;
                user.Preferences.MaxDistanceKm = dto.MaxDistanceKm;
                user.Preferences.PreferredGender = dto.PreferredGender;
            }

            // Сохраняем изменения
            await _context.SaveChangesAsync();

            // Возвращаем true, если все прошло успешно
            return true;
        }

        // Обновление фото пользователя (заглушка, пока без загрузки в Azure Blob Storage)
        public async Task<bool> UpdateUserPhotoAsync(int userId, IFormFile photoFile)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // TODO: Реализовать загрузку файла в Azure Blob Storage и получение URL
            string blobPhotoUrl = "https://yourblobstorage.com/path-to-photo"; // Временная заглушка

            // Сохраняем ссылку на фото в БД
            user.ProfilePhoto = new ProfilePhoto { PhotoUrl = blobPhotoUrl };
            await _context.SaveChangesAsync();

            return true;
        }

        // Генерация access-токена
        private string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                new[] 
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("id", user.Id.ToString()) // Добавляем id в токен
                },
                expires: DateTime.UtcNow.AddHours(1),  // Время жизни access-токена
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);  // Возвращаем JWT как строку
        }

        // Генерация refresh-токена
        private async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),  // Генерация уникального refresh-токена
                Expires = DateTime.UtcNow.AddDays(7),  // Срок действия refresh-токена
                UserId = user.Id  // Привязываем к пользователю
            };

            _context.RefreshTokens.Add(refreshToken);  // Сохраняем в БД
            await _context.SaveChangesAsync();
            return refreshToken.Token;  // Возвращаем refresh-токен
        }

        // Метод обновления токенов (если нужно)
        public async Task<(string AccessToken, string RefreshToken)?> RefreshAccessTokenAsync(TokenRefreshDto dto)
        {
            var existingToken = await _context.RefreshTokens.Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

            if (existingToken == null || existingToken.Expires < DateTime.UtcNow || existingToken.IsRevoked)
            {
                return null;  // Если токен не найден или срок его действия истек
            }

            // Отозвали старый refresh-токен
            existingToken.IsRevoked = true;

            var newAccessToken = GenerateAccessToken(existingToken.User);  // Генерация нового access-токена
            var newRefreshToken = await GenerateRefreshTokenAsync(existingToken.User);  // Генерация нового refresh-токена

            await _context.SaveChangesAsync();  // Сохраняем изменения
            return (newAccessToken, newRefreshToken);  // Возвращаем оба токена
        }
    }
}

