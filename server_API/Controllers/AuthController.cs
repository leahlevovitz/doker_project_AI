//using Microsoft.AspNetCore.Mvc;                    // ControllerBase, IActionResult
//using Microsoft.IdentityModel.Tokens;              // יצירת JWT
//using server_API.DAL;                               // AppDbContext
//using server_API.DTO;                               // LoginDto
//using System.IdentityModel.Tokens.Jwt;             // JwtSecurityToken, JwtSecurityTokenHandler
//using System.Security.Claims;                      // Claim, ClaimTypes
//using System.Text;                                 // Encoding
//using BCrypt.Net;                                  // בדיקת סיסמה מוצפנת

//namespace server_API.Controllers
//{
//    [ApiController]                                 // מאפשר קונטרולר Web API עם ולידציה אוטומטית
//    [Route("api/[controller]")]                     // נתיב: api/auth
//    public class AuthController : ControllerBase
//    {
//        private readonly AppDbContext _context;     // DbContext לגישה לטבלאות
//        private readonly IConfiguration _configuration; // גישה ל-appsettings.json

//        public AuthController(AppDbContext context, IConfiguration configuration)
//        {
//            _context = context;                    // מגדיר DbContext
//            _configuration = configuration;        // מגדיר IConfiguration
//        }

//        [HttpPost("login")]                        // נתיב POST api/auth/login
//        public IActionResult Login(LoginDto dto)
//        {
//            var user = _context.Users
//                .FirstOrDefault(u => u.UserName == dto.UserName); // חיפוש משתמש לפי שם משתמש

//            if (user == null)                        // אם המשתמש לא נמצא
//                return Unauthorized();               // החזר 401 Unauthorized

//            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) // בדיקת סיסמה מול ה-Hash
//                return Unauthorized();               // אם לא תואם → 401

//            var claims = new[]                         // יצירת הרשאות למשתמש בתוך JWT
//            {
//                new Claim(ClaimTypes.Name, user.UserName), // שם המשתמש
//                new Claim(ClaimTypes.Role, user.Role)      // תפקיד (manager)
//            };

//            var key = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]) // המפתח הסודי ליצירת חתימת הטוקן
//            );

//            var token = new JwtSecurityToken(          // יצירת ה-JWT בפועל
//                issuer: _configuration["Jwt:Issuer"],  // מי יצר את הטוקן
//                audience: _configuration["Jwt:Audience"], // למי הטוקן מיועד
//                claims: claims,                         // המידע שנכנס ל-JWT
//                expires: DateTime.Now.AddHours(1),      // תוקף הטוקן לשעה
//                signingCredentials:                      // חתימת הטוקן עם המפתח הסודי
//                    new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
//            );

//            return Ok(new                                // החזרת הטוקן ללקוח
//            {
//                token = new JwtSecurityTokenHandler().WriteToken(token) // המרה למחרוזת JWT
//            });
//        }
//    }
//}
