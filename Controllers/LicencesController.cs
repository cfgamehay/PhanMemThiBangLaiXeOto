using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiThiBangLaiXeOto.Controllers
{
    [ApiController]
    [Route("api/licences")]
    public class LicencesController : Controller
    {

        private readonly SqlHelper _sql;
        public LicencesController(SqlHelper sql)
        {
            _sql = sql;
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

        [HttpPost("{id}")]
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

        private async Task<int> InsertLicence(LicenceCreateDto dto, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                INSERT INTO Licence (LicenceCode, QuestionCount, Duration, PassScore)
                OUTPUT INSERTED.ID
                VALUES (@licence_code, @question_count, @duration, @pass_score);
             ";
            var parameters = new[]
            {
                new SqlParameter("@licence_code", SqlDbType.NVarChar){Value = dto.LicenceCode.ToUpper()},
                new SqlParameter("@question_count", SqlDbType.Int){ Value = dto.QuestionCount},
                new SqlParameter("@duration", SqlDbType.Int){ Value = dto.Duration},
                new SqlParameter("@pass_score", SqlDbType.Int){ Value = dto.PassScore}
            };

            var result = await _sql.ExecuteScalarAsync(query, conn, trans, parameters);
            return Convert.ToInt32(result);
        }

        private async Task InsertLicenceRules(int licenceId, List<LicenceRuleDto> dtos, SqlConnection conn, SqlTransaction trans)
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

        private async Task UpdateLicenceRule(int id, List<LicenceRuleDto> dtos, SqlConnection conn, SqlTransaction trans)
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
    }
}
