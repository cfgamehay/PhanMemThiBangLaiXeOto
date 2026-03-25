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
    [Route("api/VanBang")]
    public class LicencesController : Controller
    {
        //var user = await authHelper.GetUser(User, _sql);

        private readonly SqlHelper _sql;
        public LicencesController(SqlHelper sql)
        {
            _sql = sql;
        }
        //[Authorize(AuthenticationSchemes = "BearerMain")]
        [HttpGet]
        public async Task<IActionResult> GetLicences()
        {
            string query = @"
                SELECT 
                l.id as Id,
                LicenceCode,
                l.QuestionCount as TotalQuestion,
                Duration,
                PassScore,
                c.name as CategoryName,
                lr.QuestionCount,
                c.Id as CategoryId
                FROM Licence l
                left join LicenceRule lr on l.id = lr.LicenceId
                left join Category c on lr.CategoryId = c.Id
                WHERE l.IsEnable = 1
            ";
            var rawList = await _sql.ExecuteQueryAsync(query, LicenceMapper.ToRawLicenceListDto);
            var finalList = MergeLicenceList(rawList);

            
            return Ok(finalList);
        }



        [HttpPost]
        public async Task<IActionResult> CreateLicence([FromBody] LicenceCreateDto dto)
        {

            if (dto == null) {
                return BadRequest("Thông tin không hợp lệ");
            }

            using (var connection = new SqlConnection(_sql._connectionString))
            {
                await connection.OpenAsync();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        int licenceId = await InsertLicence(dto, connection, trans);
                        await InsertLicenceRules(licenceId, dto.LicenceRule, connection, trans);
                        await trans.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        return BadRequest(ex.Message);
                    }
                }
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] LicenceCreateDto dto, int id)
        {

            if (dto == null)
            {
                return BadRequest("Thông tin không hợp lệ");
            }

            using (var connection = new SqlConnection(_sql._connectionString))
            {
                await connection.OpenAsync();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        await UpdateLicence(id, dto, connection, trans);
                        await UpdateLicenceRule(id, dto.LicenceRule, connection, trans);
                        await trans.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        return BadRequest(ex.Message);
                    }
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLicence(int id)
        {
            using (var connection = new SqlConnection(_sql._connectionString))
            {
                await connection.OpenAsync();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        var query = "UPDATE Licence SET IsEnable = 0 WHERE Id = @id";
                        var parameters = new[]
                        {
                            new SqlParameter("@id", SqlDbType.Int){ Value = id}
                        };

                        await _sql.ExecuteNonQueryAsync(query, connection, trans, parameters);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        return BadRequest(ex.Message);
                    }
                }
            }
            return NoContent();
        }

        private async Task<int> InsertLicence(LicenceCreateDto dto, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                INSERT INTO Licence (LicenceCode, QuestionCount, Duration, PassScore, IsEnable)
                OUTPUT INSERTED.ID
                VALUES (@licence_code, @question_count, @duration, @pass_score, @is_enable);
             ";
            var parameters = new[]
            {
                new SqlParameter("@licence_code", SqlDbType.NVarChar){Value = dto.LicenceCode.ToUpper()},
                new SqlParameter("@question_count", SqlDbType.Int){ Value = dto.QuestionCount},
                new SqlParameter("@duration", SqlDbType.Int){ Value = dto.Duration},
                new SqlParameter("@pass_score", SqlDbType.Int){ Value = dto.PassScore},
                new SqlParameter("@is_enable", SqlDbType.Bit){ Value = true},
            };

            var result = await _sql.ExecuteScalarAsync<int>(query, conn, trans, parameters);
            return Convert.ToInt32(result);
        }

        private async Task InsertLicenceRules(int licenceId, List<LicenceRuleCreateDto> dtos, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                INSERT INTO LicenceRule (LicenceId, CategoryId, QuestionCount)
                VALUES (@licence_id, @category_id, @question_count);
             ";
            foreach (var licenceRule in dtos)
            {
                var parameters = new[]
                {
                    new SqlParameter("@licence_id", SqlDbType.Int){ Value = licenceId},
                    new SqlParameter("@category_id", SqlDbType.Int){ Value = licenceRule.CategoryId},
                    new SqlParameter("@question_count", SqlDbType.Int){ Value = licenceRule.QuestionCount},
                };
                await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
            }
        }

        private async Task UpdateLicence(int id, LicenceCreateDto dto, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                UPDATE Licence
                SET LicenceCode = @licence_code,
                    QuestionCount = @question_count,
                    Duration = @duration,
                    PassScore = @pass_score
                WHERE Id = @id
             ";

            var parameters = new[]
            {
                new SqlParameter("@licence_code", SqlDbType.NVarChar){Value = dto.LicenceCode.ToUpper()},
                new SqlParameter("@question_count", SqlDbType.Int){ Value = dto.QuestionCount},
                new SqlParameter("@duration", SqlDbType.Int){ Value = dto.Duration},
                new SqlParameter("@pass_score", SqlDbType.Int){ Value = dto.PassScore},
                new SqlParameter("@id", SqlDbType.Int){ Value = id}
            };
            await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
        }

        private async Task UpdateLicenceRule(int id, List<LicenceRuleCreateDto> dtos, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                UPDATE LicenceRule
                SET QuestionCount = @question_count
                WHERE LicenceId = @licence_id
                AND CategoryId = @category_id

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO LicenceRule(LicenceId, CategoryId, QuestionCount)
                    VALUES (@licence_id, @category_id, @question_count)
                END
             ";

            foreach (var licenceRule in dtos)
            {
                var parameters = new[]
                {
                    new SqlParameter("@licence_id", SqlDbType.Int){ Value = id},
                    new SqlParameter("@category_id", SqlDbType.Int){ Value = licenceRule.CategoryId},
                    new SqlParameter("@question_count", SqlDbType.Int){ Value = licenceRule.QuestionCount},
                };
                await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
            }
        }

        private List<object> MergeLicenceList(List<RawLicenceListDto> rawList)
        {
            return rawList
                .GroupBy(l => l.Id) // Nhóm theo LicenceId
                .Select(g => new {
                    LicenceId = g.Key,
                    LicenceCode = g.First().LicenceCode,
                    QuestionCount = g.First().TotalQuestion,
                    Duration = g.First().Duration,
                    PassScore = g.First().PassScore,
                    // Gom tất cả các Category và QuestionCount vào một danh sách Rules
                    LicenceRules = g
                        .Where(x => !string.IsNullOrEmpty(x.CategoryName)) // Bỏ qua dòng NULL của Left Join
                        .Select(r => new {
                            CategoryId = r.CategoryId,
                            CategoryName = r.CategoryName,
                            QuestionCount = r.QuestionCount
                        })
                        .ToList()
                })
                .Cast<object>()
                .ToList();
        }
    }
}
