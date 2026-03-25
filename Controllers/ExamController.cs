using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using ApiThiBangLaiXeOto.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiThiBangLaiXeOto.Controllers
{
    [ApiController]
    [Route("api/KiemTra")]
    public class ExamController : Controller
    {
        //Todo Post nhận data tu nguoi dung roi luu vao server

        private readonly SqlHelper _sql;

        public ExamController(SqlHelper sql)
        {
            _sql = sql;
        }

        [Route("CauTruc")]
        [HttpGet]
        public async Task<IActionResult> GetExamWithStructure([FromQuery] string BangLai = "B")
        {
            try
            {
                var query = "GenerateExamWithStructure";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@LicenceCode", BangLai)
                };

                List<ExamRawDto> RawList = await _sql.ExecuteQueryAsync(query, ExamMapper.ToRawQuestionListDto, parameters, System.Data.CommandType.StoredProcedure);

                if (RawList.Count == 0)
                {
                    return NotFound("Bằng lái không tồn tại hoặc chưa có câu hỏi.");
                }
                else
                {
                    var firstLine = RawList.FirstOrDefault();
                    var finalList = MergeExamList(RawList);

                    var result = new
                    {
                        LicenceCode = firstLine?.LicenceCode ?? "Unknown",
                        Duration = firstLine?.Duration ?? 0,
                        QuestionCount = finalList.Count,
                        Questions = finalList
                    };
                    return Ok(result);
                }


            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [Route("NgauNhien")]
        [HttpGet]
        public async Task<IActionResult> GetRandomExam([FromQuery] int SoLuong = 30)
        {

            if (SoLuong <= 0) SoLuong = 10;

            try
            {
                var query = "GenerateExamRandom";
                var parameters = new SqlParameter[]
                {

            new SqlParameter("@Quantity", SqlDbType.Int) { Value = SoLuong }
                };

                var RawList = await _sql.ExecuteQueryAsync(query, ExamMapper.ToRawQuestionListDto, parameters, CommandType.StoredProcedure);

                if (RawList == null || RawList.Count == 0)
                {
                    return NotFound(new { Message = "Không tìm thấy câu hỏi phù hợp." });
                }

                var firstLine = RawList.FirstOrDefault();
                var finalList = MergeExamList(RawList);

                var result = new
                {
                    LicenceCode = firstLine?.LicenceCode ?? "Unknown",
                    Duration = firstLine?.Duration ?? (SoLuong * 1.5), // Fallback nếu SQL ko tính
                    QuestionCount = finalList.Count,
                    Questions = finalList,
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây nếu cần
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [Route("NopBai")]
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitExam([FromBody] ExamSubmitFormDto submitForm)
        {
            if (submitForm == null || submitForm.Answers == null || submitForm.Answers.Count == 0)
            {
                return BadRequest(new { Message = "Dữ liệu gửi lên không hợp lệ." });
            }

            UserExamResultDto? examResult = null;

            try
            {

                var QuestionAndAnswerIds = submitForm.Answers.Select(a => $"({a.QuestionId}, {a.AnswerId})").ToList();
                var RawString = String.Join(",", QuestionAndAnswerIds);

                var query = @$"
                SELECT 
                    COUNT(*) as TotalQuestion,
                    COUNT(CASE WHEN a.IsCorrect = 1 THEN 1 END) as TotalCorrect,
                    MAX(CASE WHEN (a.IsCorrect = 0 OR a.IsCorrect IS NULL) AND q.IsCritical = 1 THEN 1 ELSE 0 END) as HitCritical
                FROM (VALUES {RawString}) AS UserSubmission(QId, AId)
                JOIN Question q ON UserSubmission.QId = q.Id
                LEFT JOIN Answer a ON UserSubmission.AId = a.Id AND a.QuestionId = q.Id
                ";
                ExamResultDto? result = await _sql.ExecuteQuerySingleAsync(query, ExamResultMapper.ToExamResultDto);


                if (result == null)
                {
                    return BadRequest(new { Message = "Không thể tính điểm do dữ liệu không hợp lệ." });
                }
                result.TotalQuestion = QuestionAndAnswerIds.Count();

                bool isPassed = result.TotalCorrect >= Math.Ceiling(QuestionAndAnswerIds.Count() * 0.9) && result.HitCritical == 0;

                examResult = new UserExamResultDto
                {
                    UserId = 1, // Lấy từ token sau khi có auth
                    LicenceId = submitForm.LicenceId,
                    TotalCorrect = result.TotalCorrect,
                    TotalQuestion = result.TotalQuestion,
                    HitCritical = result.HitCritical > 0 ? true : false,
                    IsPassed = isPassed,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });

            }

            if (examResult != null)
            {
                using (var connection = new SqlConnection(_sql._connectionString))
                {
                    await connection.OpenAsync();
                    using (var trans = connection.BeginTransaction())
                    {
                        try
                        {
                            int ExamResultId = await InsertExamResult(examResult, connection, trans);
                            await InsertExamResultDetail(ExamResultId, submitForm.Answers, connection, trans);

                            trans.Commit();
                            return Ok();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            return StatusCode(500, new { Message = "Lỗi khi lưu kết quả: " + ex.Message });
                        }
                    }
                }
            }

            return BadRequest(new { Message = "Không thể lưu kết quả do dữ liệu không hợp lệ." });
        }
        [Authorize]
        [HttpGet("LichSu")]
        public async Task<IActionResult> GetExamHistory()
        {
            try
            {
                var user = await authHelper.GetUser(User, _sql);

                if (user == null)
                {
                    return Unauthorized(new { Message = "Người dùng không hợp lệ." });
                }

                var query = "GetExamHistoryByUserId";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", user.Id)
                };
                var history = await _sql.ExecuteQueryAsync(query, ExamResultMapper.ToExamHistoryDto, parameters, CommandType.StoredProcedure);

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [Authorize]
        [HttpGet("LichSu/{id}")]
        public async Task<IActionResult> GetExamDetailHistory(int id)
        {
            try
            {
                var user = await authHelper.GetUser(User, _sql);

                if (user == null)
                {
                    return Unauthorized(new { Message = "Người dùng không hợp lệ." });
                }

                var query = "GetExamHistoryDetailByUserId";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserId", user.Id),
                    new SqlParameter("@ExamId", id)
                };
                var historyDetail = await _sql.ExecuteQueryAsync(query, ExamResultMapper.ToRawExamHistoryDetail, parameters, CommandType.StoredProcedure);
                var finalHistoryDetail = MergeExamHistoryDetail(historyDetail);
                return Ok(finalHistoryDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        private List<ExamDto> MergeExamList(List<ExamRawDto> rawList)
        {
            var finalList = rawList
                .GroupBy(q => q.QuestionId)
                .Select(g =>
                {
                    var first = g.First();
                    return new ExamDto
                    {
                        Id = g.Key,
                        QuestionContent = first.QuestionContent,
                        ImageUrl = first.ImageUrl,
                        Answers = g.GroupBy(a => a.AnswerId).Select(ga => new ExamAnswerDto
                        {
                            Id = ga.Key,
                            AnswerContent = ga.First().AnswerContent,
                        }).ToList()
                    };
                }).ToList();
            return finalList;
        }

        private async Task<int> InsertExamResult(UserExamResultDto examResult, SqlConnection connection, SqlTransaction transaction)
        {
            var query = "INSERT INTO Exam (UserId, LicenceId, TotalCorrect, IsFailedByCritical, IsPassed, CreatedAt, TotalQuestion) " +
                        "OUTPUT INSERTED.Id " +
                        "VALUES (@UserId, @LicenceId, @TotalCorrect, @HitCritical, @IsPassed, @CreatedAt, @TotalQuestion)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", examResult.UserId),
                new SqlParameter("@LicenceId", examResult.LicenceId ?? (object)DBNull.Value),
                new SqlParameter("@TotalCorrect", examResult.TotalCorrect),
                new SqlParameter("@HitCritical", examResult.HitCritical),
                new SqlParameter("@IsPassed", examResult.IsPassed),
                new SqlParameter("@CreatedAt", examResult.CreatedAt),
                new SqlParameter("@TotalQuestion", examResult.TotalQuestion)
            };

            int result = await _sql.ExecuteScalarAsync<int>(query, connection, transaction, parameters, CommandType.Text);

            return result;
        }

        private async Task InsertExamResultDetail(int ExamResultId, List<ExamAnswerSubmitDto> answers, SqlConnection connection, SqlTransaction transaction)
        {
            if (answers == null || !answers.Any()) return;

            var valueList = answers.Select(a => $"({ExamResultId}, {a.QuestionId}, {a.AnswerId})").ToList();
            var bulkQuery = $"INSERT INTO ExamDetail (ExamId, QuestionId, AnswerId) VALUES {string.Join(",", valueList)}";
            await _sql.ExecuteNonQueryAsync(bulkQuery, connection, transaction);
        }

        private List<ExamHistoryDetailDto> MergeExamHistoryDetail(List<RawExamHistoryDetailDto> rawList)
        {
            var mergedList = rawList
            .GroupBy(q => new { q.QuestionId, q.QuestionContent, q.ImageLink, q.IsCritical, q.Explain })
            .Select(group => new ExamHistoryDetailDto
            {
                QuestionId = group.Key.QuestionId,
                QuestionContent = group.Key.QuestionContent,
                ImageLink = group.Key.ImageLink,
                IsCritical = group.Key.IsCritical,
                Explain = group.Key.Explain,
                Answers = group.Select(a => new ExamHistoryAnswerDto
                {
                    AnswerId = a.AnswerId,
                    AnswerContent = a.AnswerContent,
                    CorrectAnswer = a.CorrectAnswer,
                    IsSelected = a.IsSelected
                }).ToList()
            })
            .ToList();

            return mergedList;
        }
    }
}
