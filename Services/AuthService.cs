using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrepenAPI.Data;
using PrepenAPI.DTOs;
using PrepenAPI.Models;

namespace PrepenAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(AdminLoginDto loginDto);
        Task<AuthResponseDto?> RegisterAsync(AdminRegisterDto registerDto);
        string GenerateJwtToken(Admin admin);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> LoginAsync(AdminLoginDto loginDto)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == loginDto.Username);

            if (admin == null || !VerifyPasswordHash(loginDto.Password, admin.PasswordHash, admin.PasswordSalt))
            {
                return null;
            }

            var token = GenerateJwtToken(admin);

            return new AuthResponseDto
            {
                Token = token,
                Username = admin.Username
            };
        }

        public async Task<AuthResponseDto?> RegisterAsync(AdminRegisterDto registerDto)
        {
            if (await _context.Admins.AnyAsync(a => a.Username == registerDto.Username))
            {
                return null; // Username already exists
            }

            CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var admin = new Admin
            {
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(admin);

            return new AuthResponseDto
            {
                Token = token,
                Username = admin.Username
            };
        }

        public string GenerateJwtToken(Admin admin)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }
}
