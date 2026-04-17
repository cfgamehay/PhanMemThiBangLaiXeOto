namespace ApiThiBangLaiXeOto.Middleware
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Security.Claims;

    public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
    {
        //1 = Admin, 0 = User thường
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Lấy role từ claim
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            // Kiểm tra: Nếu không phải "1" VÀ cũng không phải "ADMIN" thì mới cấm
            // Lưu ý: Dùng Equals kèm StringComparison để tránh lỗi viết hoa/thường
            bool isAdmin = roleClaim == "1" ||
                           (roleClaim != null && roleClaim.Equals("ADMIN", StringComparison.OrdinalIgnoreCase));

            if (!isAdmin)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
