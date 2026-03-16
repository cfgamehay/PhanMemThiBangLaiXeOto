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

            // chưa đăng nhập
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // lấy role từ claim
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            // không phải admin
            if (role != "1")
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
