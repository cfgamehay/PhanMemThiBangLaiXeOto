using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.Helper;
using ApiThiBangLaiXeOto.Middleware;
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
        public IActionResult Profile()
        {
            try
            {
                return Ok(new
                {
                    success = true,
                    data = authHelper.GetUser(User, _sql).Result
                });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Không tải được thông tin" });
            }
        }

        [AdminOnly]
        [HttpGet("Admin")]
        public IActionResult Admin()
        {
            return Ok(new { message = "Đây là trang Admin" });
        }
    }
}