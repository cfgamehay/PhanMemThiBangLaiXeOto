using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using System.Security.Claims;

namespace ApiThiBangLaiXeOto.Helper
{
    public static class authHelper
    {
        public static async Task<UserDTO?> GetUser(ClaimsPrincipal? principal, SqlHelper _sql)
        {
            // 1. Kiểm tra principal có null không (Tránh warn CS8600)
            if (principal == null) return null;

            // 2. Tìm Claim chứa ID (Sử dụng toán tử ?. để an toàn)
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            // 3. Kiểm tra giá trị Claim và ép kiểu
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                // Truy vấn DB (Kết quả có thể là null nên UserDTO? là kiểu trả về phù hợp)
                return await _sql.GetUserAsync(userId, null);
            }

            return null;
        }
    }
}