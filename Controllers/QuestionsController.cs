using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using ApiThiBangLaiXeOto.Interface;
using ApiThiBangLaiXeOto.Mapper;
using ApiThiBangLaiXeOto.Middleware;
using ApiThiBangLaiXeOto.Service;
using ApiThiBangLaiXeOto.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace ApiThiBangLaiXeOto.Controllers
{
    [ApiController]
    [Route("api/CauHoi")]
    public class QuestionsController : Controller
    {
        private readonly SqlHelper _sql;
        private readonly IQuestionService _questionService;
        public QuestionsController(SqlHelper sql, IQuestionService questionService)
        {
            _sql = sql;
            _questionService = questionService;
        }
        [HttpGet]
        public async Task<IActionResult> GetQuestions([FromQuery] int? Chuong, bool? CauDiemLiet, bool? BienBao, int? SoLuong = 60, int? Trang = 1, string? TuKhoa = null)
        {
            int pageSize = Math.Max(1, SoLuong ?? 60);
            int page = Math.Max(1, Trang ?? 1);
            int offset = (page - 1) * pageSize;

            var whereClauses = new List<string>();
            var questionFilterParam = new List<SqlParameter>();
            var questionCountParam = new List<SqlParameter>();

            // Only enabled questions
            whereClauses.Add("q.IsEnable <> 0");
            if (!string.IsNullOrEmpty(TuKhoa))
            {
                TuKhoa = TuKhoa.Trim();
                whereClauses.Add("q.Content LIKE @search OR q.Id LIKE @search");
                questionFilterParam.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = $"%{TuKhoa}%" });
                questionCountParam.Add(new SqlParameter("@search", SqlDbType.NVarChar) { Value = $"%{TuKhoa}%" });
            }
            if (CauDiemLiet.HasValue)
            {
                whereClauses.Add("q.IsCritical = @isCritical");
                questionFilterParam.Add(new SqlParameter("@isCritical", SqlDbType.Bit) { Value = CauDiemLiet.Value });
                questionCountParam.Add(new SqlParameter("@isCritical", SqlDbType.Bit) { Value = CauDiemLiet.Value });
            }
            if (Chuong.HasValue)
            {
                whereClauses.Add("qc.CategoryId = @chapter");
                questionFilterParam.Add(new SqlParameter("@chapter", SqlDbType.Int) { Value = Chuong.Value });
                questionCountParam.Add(new SqlParameter("@chapter", SqlDbType.Int) { Value = Chuong.Value });
            }
            if (BienBao.HasValue && BienBao.Value == true)
            {
                whereClauses.Add("q.ImageLink <> ''");
            }


            var whereSql = whereClauses.Count > 0
                ? "WHERE " + string.Join(" AND ", whereClauses)
                : string.Empty;

            string queryCount = $@"
                SELECT COUNT(DISTINCT q.Id)
                FROM question q
                LEFT JOIN QuestionCategory qc ON qc.QuestionId = q.Id
                {whereSql}
            ";

            int totalQuestion = await _sql.ExecuteScalarAsync<int>(queryCount, questionCountParam.ToArray());
            questionFilterParam.Add(new SqlParameter("@offset", SqlDbType.Int) { Value = offset });
            questionFilterParam.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
            // ... (Giữ nguyên phần khởi tạo whereClauses và params)

            string queryGetQuestions = $@"
    WITH PagedQuestions AS (
        SELECT DISTINCT q.Id
        FROM question q
        INNER JOIN QuestionCategory qc ON qc.QuestionId = q.Id -- Dùng INNER JOIN để chắc chắn có chương
        {whereSql}
        ORDER BY q.Id
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
    )
    SELECT 
        q.Id           AS QuestionId,
        q.Content      AS QuestionContent,
        q.Explain      AS Explanation,
        q.ImageLink    AS ImageUrl,
        qc.CategoryId  AS CategoryId,
        a.Id           AS AnswerId,
        a.Content      AS AnswerContent,
        a.IsCorrect    AS IsCorrect,
        q.IsCritical   AS IsCritical
    FROM question AS q
    INNER JOIN answer a ON a.QuestionId = q.Id
    INNER JOIN QuestionCategory qc ON qc.QuestionId = q.Id -- Đổi thành INNER JOIN
    WHERE q.Id IN (SELECT Id FROM PagedQuestions) 
    AND a.IsEnable <> 0
    {(Chuong.HasValue ? "AND qc.CategoryId = @chapter" : "")} -- Lọc lại chương ở đây nếu cần chính xác tuyệt đối
    ORDER BY q.Id, a.Id;";
            // Count total questions for pagination

            var rawList = await _sql.ExecuteQueryAsync(queryGetQuestions, QuestionMapper.ToRawQuestionListDto, questionFilterParam.ToArray());
            var finalList = MergeQuestionList(rawList);

            //Custom response for pagination
            var QuestionRespone = new
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalQuestion / pageSize),
                QuestionCount = finalList.Count(),
                Questions = finalList
            };

            return Ok(QuestionRespone);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionById(int id)
        {
            var query = @"
                SELECT
                    q.Id           AS QuestionId,
                    q.Content     AS QuestionContent,
                    q.Explain      AS Explanation,
                    q.ImageLink   AS ImageUrl,
                    qc.CategoryId  AS CategoryId,
                    a.Id           AS AnswerId,
                    a.Content     AS AnswerContent,
                    a.IsCorrect    AS IsCorrect,
                    q.IsCritical   AS IsCritical
                FROM question AS q
                JOIN answer a 
                    ON a.QuestionId = q.id
                LEFT JOIN QuestionCategory qc 
                    ON qc.QuestionId = q.id
                WHERE q.Id = @id AND q.IsEnable <> 0 AND a.IsEnable <> 0;
            ";
            var parameters = new[]
            {
                new SqlParameter("@id", SqlDbType.Int){ Value = id}
            };
            var rawList = await _sql.ExecuteQueryAsync(query, QuestionMapper.ToRawQuestionListDto, parameters);
            var finalList = MergeQuestionList(rawList);
            if (finalList.Count == 0)
                return NotFound($"Không tìm thấy câu hỏi với id {id}");
            return Ok(finalList.First());
        }

        [HttpGet("CauTruc")]
        public async Task<IActionResult> GetStructureExam([FromQuery] string BangLai = "B")
        {
            var query = "GenerateQuestionWithStructure";
            try
            {
                var parameters = new[]
{
                new SqlParameter("@LicenceCode", SqlDbType.NVarChar) { Value = BangLai }
            };

                var rawList = await _sql.ExecuteQueryAsync(
                query,
                QuestionMapper.ToRawQuestionListDto,
                parameters,
                CommandType.StoredProcedure
                );

                var firstLine = rawList.FirstOrDefault();

                var finalList = MergeQuestionList(rawList);

                var QuestionRespone = new
                {
                    LicenceCode = firstLine?.LicenceCode ?? "Unknown",
                    Duration = firstLine?.Duration ?? 0,
                    QuestionCount = finalList.Count(),
                    Questions = finalList
                };
                return Ok(QuestionRespone);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("NgauNhien")]
        public async Task<IActionResult> GetRandomExam([FromQuery] int SoLuong = 30)
        {
            var query = "GenerateQuestionRandom";

            try
            {
                if(SoLuong <= 0)
                {
                    SoLuong = 10;
                }

                var parameters = new[]
{
                new SqlParameter("@Quantity", SqlDbType.Int) { Value = SoLuong }
            };

                var rawList = await _sql.ExecuteQueryAsync(
                query,
                QuestionMapper.ToRawQuestionListDto,
                parameters,
                CommandType.StoredProcedure
                );
                var firstLine = rawList.FirstOrDefault();

                var finalList = MergeQuestionList(rawList);

                var QuestionRespone = new
                {
                    LicenceCode = firstLine?.LicenceCode ?? "Unknown",
                    Duration = firstLine?.Duration ?? 0,
                    QuestionCount = finalList.Count(),
                    Questions = finalList
                };
                return Ok(QuestionRespone);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [HttpPost("json")]
        //Get List of QuestionCreateDTO from json response
        public async Task<IActionResult> Create([FromBody] List<QuestionCreateDTO> dtos)
        {
            var CreatedQuestionInfoList = new List<object>();

            //Check if empty
            if (dtos == null || !dtos.Any())
            {
                return BadRequest("Danh sách câu hỏi không hợp lệ.");
            }

            //simple validate

            using (var connection = new SqlConnection(_sql._connectionString))
            {
                await connection.OpenAsync();
                using (var trans = connection.BeginTransaction())

                    try
                    {
                        foreach (var dto in dtos)
                        {
                            if (string.IsNullOrEmpty(dto.Question) || dto.Answers == null || !dto.Answers.Any())
                            {
                                CreatedQuestionInfoList.Add(new { Message = "Câu hỏi không hợp lệ.", QuestionContent = dto });
                                continue;
                            }

                            int questionId = await _questionService.InsertQuestion(dto, connection, trans);
                            await _questionService.InsertAnswers(questionId, dto.Answers, connection, trans);
                            await _questionService.InsertQuestionCategory(questionId, dto.CategoryIds, connection, trans);
                            CreatedQuestionInfoList.Add(new { QuestionId = questionId, Message = "Câu hỏi đã được tạo thành công." });
                        }
                        await trans.CommitAsync();
                    }

                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        //return StatusCode(500, $"An error occurred while creating the question: {ex.Message}");
                        CreatedQuestionInfoList.Add(new { Message = $"Có lỗi xảy ra trong quá trình tạo câu hỏi {ex.Message}", });
                    }
            }
            return Ok(CreatedQuestionInfoList);
        }

        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [HttpPost("form")]
        public async Task<IActionResult> CreateFromForm([FromForm] QuestionCreateDTO dto)
        {

            if (string.IsNullOrEmpty(dto.Question) || dto.Answers == null || !dto.Answers.Any())
            {
                return BadRequest("Câu hỏi không hợp lệ.");
            }
            else
            {
                string fileName = String.Empty;
                using (var connection = new SqlConnection(_sql._connectionString))
                {
                    await connection.OpenAsync();
                    using (var trans = connection.BeginTransaction())
                    {
                        try
                        {
                            if (dto.Image != null)
                            {
                                fileName = await ImageUtils.UploadImage(dto.Image);
                                dto.ImageLink = fileName;
                            }

                            int questionId = await _questionService.InsertQuestion(dto, connection, trans);
                            await _questionService.InsertAnswers(questionId, dto.Answers, connection, trans);
                            await _questionService.InsertQuestionCategory(questionId, dto.CategoryIds, connection, trans);
                            await trans.CommitAsync();

                        }
                        catch (Exception ex)
                        {
                            await trans.RollbackAsync();

                            if (!string.IsNullOrEmpty(fileName))
                            {
                                ImageUtils.DeleteImage(fileName);
                            }
                            return StatusCode(500, $"Tạo câu hỏi không thành công: {ex.Message}");

                        }
                    }
                }
            }


            return Created();
        }
        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromForm] QuestionUpdateDto dto)
        {
            using (var connection = new SqlConnection(_sql._connectionString))
            {
                await connection.OpenAsync();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        await _questionService.UpdateQuestion(id, dto, connection, trans);
                        await _questionService.UpdateAnswers(id, dto.Answers, dto.AnswerDeletedIds, connection, trans);
                        await _questionService.UpdateQuestionCategory(id, dto.CategoryIds, connection, trans);
                        await trans.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();

                        return StatusCode(500, $"An error occurred while creating the question: {ex.Message}");
                    }
                }
            }
            return NoContent();
        }
        [Authorize(AuthenticationSchemes = "BearerMain")]
        [AdminOnly]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var query = @"
                UPDATE Question
                SET IsEnable = 0
                WHERE Id = @id
            ";

            var parameters = new[]{
                new SqlParameter("@id", SqlDbType.Int){Value = id}
            };

            await _sql.ExecuteNonQueryAsync(query, parameters);
            return NoContent();
        }

        private List<QuestionDto> MergeQuestionList(List<QuestionRawDto> rawList)
        {
            var finalList = rawList
                .GroupBy(q => q.QuestionId)
                .Select(g =>
                {
                    var first = g.First();
                    return new QuestionDto
                    {
                        Id = g.Key,
                        QuestionContent = first.QuestionContent,
                        Explanation = first.Explanation,
                        ImageUrl = first.ImageUrl,
                        Categories = g.Select(x => x.CategoryId).Distinct().ToList(),
                        IsCritical = first.IsCritical,
                        Answers = g.GroupBy(a => a.AnswerId).Select(ga => new AnswerDto
                        {
                            Id = ga.Key,
                            AnswerContent = ga.First().AnswerContent,
                            IsCorrect = ga.First().IsCorrect
                        }).ToList()
                    };
                }).ToList();
            return finalList;
        }
    }
}

