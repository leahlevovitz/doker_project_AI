using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using server_API.DAL;
using server_API.DTO;
using server_API.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IConfiguration config, ILogger<AuthController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (await _context.Users.AnyAsync(u => u.UserName == dto.userName))
                    return BadRequest("Username already exists");

                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                    return BadRequest("Email already exists");

                var user = new User
                {
                    Name = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
                    Role = "client",
                    UserName = dto.userName,
                    adress = dto.adress,
                    phone = dto.phone
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {UserName}", dto.userName);
                return Ok(new { message = "Registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for {UserName}", dto.userName);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {UserName}", dto.UserName);
                    return Unauthorized("Invalid credentials");
                }

                var token = GenerateToken(user);
                _logger.LogInformation("User logged in: {UserName}", dto.UserName);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {UserName}", dto.UserName);
                return StatusCode(500, "Internal Server Error");
            }
        }

        private string GenerateToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogCritical("JWT Key is missing in configuration!");
                throw new InvalidOperationException("Security key not configured");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}