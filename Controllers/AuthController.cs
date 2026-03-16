using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiThiBangLaiXeOto.Controllers
{
    [Route("Auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly SqlHelper _sql;
        public AuthController(IConfiguration config, SqlHelper sql)
        {
            _config = config;
            _sql = sql;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO dto)
        {
            bool flag = await _sql.CheckUserExistsAsync(dto.Username);
            if (flag)
            {
                return BadRequest(new { message = "User already exists" });
            }
            var password = PasswordHasher.HashPassword(dto.Password);
            await _sql.InsertUserAsync(dto.Username, password);
            return Ok(new { message = "User created successfully" });

        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDTO dto)
        {
            // 1. Lấy hash từ DB
            var passwordHash = await _sql.GetHashPasswordAsync(dto.Username);
            if (string.IsNullOrEmpty(passwordHash))
                return Unauthorized(new { message = "Invalid login" });

            // 2. Verify password (PasswordHasher phải dùng PBKDF2/BCrypt/Argon2)
            if (!PasswordHasher.VerifyPassword(dto.Password, passwordHash))
                return Unauthorized(new { message = "Invalid login" });

            // 3. Lấy user (sau khi đã verify)
            var user = await _sql.GetUserAsync(null, dto.Username);
            if (user == null) 
                return Unauthorized(new { message = "Invalid login" });
            // kiểm tra Status
            if (!user.Status)
            {
                return Unauthorized(new
                {
                    code = "DISABLE",
                });
            }
            // 4. Tạo token (lưu ý: user.Authorize nên là tên role nếu dùng Role-based authorization)
            var token = GenerateJwtToken( user.Id, user.Role);
            return Ok(new { accessToken = token });

        }
        [Authorize(AuthenticationSchemes = "BearerMain")]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout successful. Remove token on client side." });
        }
        private string GenerateJwtToken(int iduser, int userRole)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key_Main"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, iduser.ToString()),
        new Claim(ClaimTypes.Role, userRole.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64)
    };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
