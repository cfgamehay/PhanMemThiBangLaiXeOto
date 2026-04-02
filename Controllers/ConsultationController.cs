using ApiThiBangLaiXeOto.Service;
using Microsoft.AspNetCore.Mvc;

namespace ApiThiBangLaiXeOto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultationController : ControllerBase
    {
        [HttpGet("online-users")]
        public IActionResult GetUsers()
        {
            return Ok(OnlineStore.Users.Values);
        }
    }
}