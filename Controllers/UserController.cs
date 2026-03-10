using ApiThiBangLaiXeOto.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiThiBangLaiXeOto.Controllers
{
    [Authorize(AuthenticationSchemes = "BearerMain")]
    [Route("User")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly SqlHelper _sql;
        public UserController(SqlHelper sql) 
        { 
            _sql = sql;
        }
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] string pass)
        {
            if (string.IsNullOrWhiteSpace(pass))
                return BadRequest(new { Success = false, Message = "Password không hợp lệ." });
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                await _sql.UpdatePassword(userId, pass);
                return Ok(new { Success = true, Message = "Đổi mật khẩu thành công." });
            }
            return StatusCode(500, new { Success = false, Message = "Có lỗi xảy ra khi đổi mật khẩu." });
        }
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var user = await _sql.GetUserAsync(userId, null);
                return Ok(new
                {
                    success = true,
                    data = user
                });
            }
            return BadRequest(new { message = "Không tải được thông tin" });
        }
    }
}
