using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using ApiThiBangLaiXeOto.Mapper;
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
        public QuestionsController(SqlHelper sql)
        {
            _sql = sql;
        }
        [HttpGet]
        public async Task<IActionResult> GetQuestions([FromQuery] int? Chuong, bool? CauDiemLiet, bool? BienBao, int? SoLuong = 60, int? Trang = 1)
        {
            int pageSize = Math.Max(1, SoLuong ?? 60);
            int page = Math.Max(1, Trang ?? 1);
            int offset = (page - 1) * pageSize;

            var whereClauses = new List<string>();
            var questionFilterParam = new List<SqlParameter>();
            var questionCountParam = new List<SqlParameter>();

            // Only enabled questions
            whereClauses.Add("q.IsEnable <> 0");

            if (CauDiemLiet.HasValue)
            {
                whereClauses.Add("q.IsCritical = @isCritical");
                questionFilterParam.Add(new SqlParameter("@isCritical", SqlDbType.Bit) { Value = CauDiemLiet.Value });
                questionCountParam.Add(new SqlParameter("@isCritical", SqlDbType.Bit) { Value = CauDiemLiet.Value });
            }
            else if (Chuong.HasValue)
            {
                whereClauses.Add("qc.CategoryId = @chapter");
                questionFilterParam.Add(new SqlParameter("@chapter", SqlDbType.Int) { Value = Chuong.Value });
                questionCountParam.Add(new SqlParameter("@chapter", SqlDbType.Int) { Value = Chuong.Value });   
            }
            else if (BienBao.HasValue && BienBao.Value == true)
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

            int totalQuestion = await _sql.ExecuteScalarAsync(queryCount, questionCountParam.ToArray());

            questionFilterParam.Add(new SqlParameter("@offset", SqlDbType.Int) { Value = offset });
            questionFilterParam.Add(new SqlParameter("@pageSize", SqlDbType.Int) { Value = pageSize });
            string queryGetQuestions = $@"
                WITH PagedQuestions AS (
                    SELECT q.Id
                    FROM question q
                    LEFT JOIN QuestionCategory qc ON qc.QuestionId = q.Id
                    {whereSql}
                    GROUP BY q.Id
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
                LEFT JOIN QuestionCategory qc ON qc.QuestionId = q.Id
                WHERE q.Id IN (SELECT Id FROM PagedQuestions)
                ORDER BY q.Id, a.Id;
            ";
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
                WHERE q.Id = @id;
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
            var query = "GenerateExam";
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

                var finalList = MergeQuestionList(rawList);

                var QuestionRespone = new
                {
                    LicenceCode = BangLai,
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
        public async Task<IActionResult> GetRandomExam([FromQuery] string BangLai = "B")
        {
            var query = "GenerateRandomExam";

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

                var finalList = MergeQuestionList(rawList);

                var QuestionRespone = new
                {
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
        [HttpPost]
        //Get List of QuestionCreateDTO from json response
        public async Task<IActionResult> Create([FromBody] List<QuestionCreateDTO> dtos)
        {
            var CreatedQuestionInfoList = new List<object>();

            //Check if empty
            if (dtos == null || !dtos.Any())
            {
                return BadRequest("Danh sách câu hỏi không hợp lệ.");
            }

            foreach (var dto in dtos)
            {
                //simple validate
                if (string.IsNullOrEmpty(dto.Question) || dto.Answers == null || !dto.Answers.Any())
                {
                    CreatedQuestionInfoList.Add(new { Message = "Câu hỏi không hợp lệ.", QuestionContent = dto });
                    continue;
                }

                using (var connection = new SqlConnection(_sql._connectionString))
                {
                    await connection.OpenAsync();
                    using (var trans = connection.BeginTransaction())
                    {
                        try
                        {
                            int questionId = await InsertQuestion(dto, connection, trans);
                            await InsertAnswers(questionId, dto.Answers, connection, trans);
                            await InsertQuestionCategory(questionId, dto.CategoryIds, connection, trans);
                            await trans.CommitAsync();

                            CreatedQuestionInfoList.Add(new { QuestionId = questionId, Message = "Câu hỏi đã được tạo thành công." });
                        }
                        catch (Exception ex)
                        {
                            await trans.RollbackAsync();
                            //return StatusCode(500, $"An error occurred while creating the question: {ex.Message}");
                            CreatedQuestionInfoList.Add(new { Message = $"Có lỗi xảy ra trong quá trình tạo câu hỏi {ex.Message}", });
                        }
                    }
                }
            }
            return Ok(CreatedQuestionInfoList);
        }

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
            return Ok(id);
        }

        private async Task<int> InsertQuestion(QuestionCreateDTO dto, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                INSERT INTO Question (Content, Explain, ImageLink, IsEnable)
                OUTPUT INSERTED.ID
                VALUES (@content, @explain, @image_link, @is_enable);
             ";
            var parameters = new[]
            {
                new SqlParameter("@content", SqlDbType.NVarChar){Value = dto.Question},
                new SqlParameter("@explain", SqlDbType.NVarChar){ Value = dto.Explain ?? string.Empty},
                new SqlParameter("@image_link", SqlDbType.NVarChar){ Value = dto.ImageLink ?? string.Empty},
                new SqlParameter("@is_enable", SqlDbType.Int){ Value = 1}
            };

            var result = await _sql.ExecuteScalarAsync(query, conn, trans, parameters);
            return Convert.ToInt32(result);
        }

        private async Task InsertAnswers(int questionId, List<AnswerCreateDto> answers, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                INSERT INTO answer (QuestionId, Content, IsCorrect, IsEnable)
                VALUES (@question_id, @content, @is_correct, @is_enable);
             ";
            foreach (var answer in answers)
            {
                var parameters = new[]
                {
                    new SqlParameter("@question_id", SqlDbType.Int){ Value = questionId},
                    new SqlParameter("@content", SqlDbType.NVarChar){ Value = answer.Text},
                    new SqlParameter("@is_correct", SqlDbType.Bit){ Value = answer.IsCorrect},
                    new SqlParameter("@is_enable", SqlDbType.Int){ Value = 1}
                };
                await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
            }
        }

        private async Task InsertQuestionCategory(int questionId, List<int> categoryIds, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                INSERT INTO QuestionCategory (QuestionId, CategoryId)
                VALUES (@question_id, @category_id);
             ";
            foreach (var categoryId in categoryIds)
            {
                var parameters = new[]
                {
                    new SqlParameter("@question_id", SqlDbType.Int){ Value = questionId},
                    new SqlParameter("@category_id", SqlDbType.Int){ Value = categoryId}
                };
                await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
            }
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

