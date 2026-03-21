using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiThiBangLaiXeOto.Controllers
{
    [ApiController]
    [Route("api/HocTap")]
    public class LearningController : Controller
    {
        private readonly SqlHelper _sql;
        public LearningController(SqlHelper sql)
        {
            _sql = sql;
        }

        [Authorize(AuthenticationSchemes = "BearerMain")]
        [Route("LuuKetQua")]
        [HttpPost]
        public async Task<IActionResult> SaveLearningProgress(SaveLearningProgressDto dto)
        {
            try
            {
                var user = await authHelper.GetUser(User, _sql);

                if (user == null)
                {
                    return Unauthorized();
                }

                int id = user.Id;

                var parameters = new SqlParameter[]
                {
                      new SqlParameter("@UserId", id),
                      new SqlParameter("@QuestionId", dto.QuestionId),
                      new SqlParameter("@IsCorrect", dto.IsCorrect),

                };

                var query = "sp_SaveUserLearningProgress";
                await _sql.ExecuteNonQueryAsync(query, parameters, CommandType.StoredProcedure);

                return Ok();
            } catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lưu kết quả học tập: {ex.Message}");
            }

        }

        [Authorize(AuthenticationSchemes = "BearerMain")]
        [Route("CauHoiYeuThich/{questionId}")]
        [HttpPost]
        public async Task<IActionResult> SetFavoriteQuestion(int questionId)
        {
            try
            {
                var user = await authHelper.GetUser(User, _sql);

                if (user == null)
                {
                    return Unauthorized();
                }

                int id = user.Id;

                var parameters = new SqlParameter[]
                {
                      new SqlParameter("@UserId", id),
                      new SqlParameter("@QuestionId", questionId)
                };

                var query = "sp_ToggleFavorite";
                await _sql.ExecuteNonQueryAsync(query, parameters, CommandType.StoredProcedure);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lưu kết quả học tập: {ex.Message}");
            }

        }
    }

    //Todo 
    //Hiển thị danh sách câu sai, hay sai or hiển thị danh sách câu đã làm gộp chung
    //hiển thị danh sách câu yêu thích
}
