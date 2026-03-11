using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using System.Security.Claims;

namespace ApiThiBangLaiXeOto.Helper
{
    public static class authHelper
    {
        public static async Task<UserDTO> GetUser(ClaimsPrincipal User, SqlHelper _sql)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                UserDTO user = await _sql.GetUserAsync(userId, null);
                return user;
            }
            return null;
        }
    }
}
