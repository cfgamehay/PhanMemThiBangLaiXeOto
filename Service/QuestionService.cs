using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Interface;
using ApiThiBangLaiXeOto.Utils;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiThiBangLaiXeOto.Service
{
    public class QuestionService : IQuestionService
    {
        private readonly SqlHelper _sql;
        public QuestionService(SqlHelper sql) {
            _sql = sql;
        }

        public async Task<int> InsertQuestion(QuestionCreateDTO dto, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                INSERT INTO Question (Content, Explain, ImageLink, IsEnable, IsCritical)
                OUTPUT INSERTED.ID
                VALUES (@content, @explain, @image_link, @is_enable, @is_critical);
             ";
            var parameters = new[]
            {
                new SqlParameter("@content", SqlDbType.NVarChar){Value = dto.Question},
                new SqlParameter("@explain", SqlDbType.NVarChar){ Value = dto.Explain ?? string.Empty},
                new SqlParameter("@image_link", SqlDbType.NVarChar){ Value = dto.ImageLink ?? string.Empty},
                new SqlParameter("@is_critical", SqlDbType.Int){ Value = dto.IsCritical ? 1 : 0},
                new SqlParameter("@is_enable", SqlDbType.Int){ Value = 1}
            };

            var result = await _sql.ExecuteScalarAsync<int>(query, conn, trans, parameters);
            return Convert.ToInt32(result);
        }

        public async Task InsertAnswers(int questionId, List<AnswerCreateDto> answers, SqlConnection conn, SqlTransaction trans)
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

        public async Task InsertQuestionCategory(int questionId, List<int> categoryIds, SqlConnection conn, SqlTransaction trans)
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

        public async Task UpdateQuestion(int id, QuestionUpdateDto dto, SqlConnection conn, SqlTransaction trans)
        {
            // Nếu có hình gửi lên
            if (dto.Image != null)
            {
                // Lấy thông tin câu hỏi cũ để xóa ảnh cũ nếu có
                string getImageQuery = "SELECT ImageLink FROM Question WHERE Id = @id";
                var getImageParam = new[] { new SqlParameter("@id", SqlDbType.Int) { Value = id } };
                var oldImageLink = await _sql.ExecuteScalarAsync<string>(getImageQuery, conn, trans, getImageParam);

                // Xử lý upload ảnh mới
                string newFileName = await ImageUtils.UploadImage(dto.Image);
                dto.ImageLink = newFileName;
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(oldImageLink))
                {
                    ImageUtils.DeleteImage(oldImageLink);
                }
            }
            else if (dto.IsImageDeleted) // Nếu có yêu cầu xóa ảnh nhưng không gửi ảnh mới
            {
                // Lấy thông tin câu hỏi cũ để xóa ảnh cũ nếu có
                string getImageQuery = "SELECT ImageLink FROM Question WHERE Id = @id";
                var getImageParam = new[] { new SqlParameter("@id", SqlDbType.Int) { Value = id } };
                var oldImageLink = await _sql.ExecuteScalarAsync<string>(getImageQuery, conn, trans, getImageParam);
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(oldImageLink))
                {
                    ImageUtils.DeleteImage(oldImageLink);
                }
                dto.ImageLink = string.Empty; // Xóa đường dẫn ảnh trong database
            }

            string query = @"
                UPDATE Question
                SET Content = @content, Explain = @explain, ImageLink = @image_link, IsEnable = @is_enable, IsCritical = @is_critical
                WHERE Id = @id;
             ";
            var parameters = new[]
            {
                new SqlParameter("@id", SqlDbType.Int){ Value = id},
                new SqlParameter("@content", SqlDbType.NVarChar){Value = dto.Question},
                new SqlParameter("@explain", SqlDbType.NVarChar){ Value = dto.Explain ?? string.Empty},
                new SqlParameter("@image_link", SqlDbType.NVarChar){ Value = dto.ImageLink ?? string.Empty},
                new SqlParameter("@is_enable", SqlDbType.Int){ Value = 1},
                new SqlParameter("@is_critical", SqlDbType.Int){ Value = dto.IsCritical},

            };

            await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
        }


        public async Task UpdateAnswers(int questionId, List<AnswerUpdateDto> answers, List<int> deletedQuestionIds, SqlConnection conn, SqlTransaction trans)
        {
            if (deletedQuestionIds.Count > 0)
            {
                var deleteQuery = @"
                    UPDATE answer
                    SET IsEnable = 0
                    WHERE Id = @id;
                ";
                foreach (var id in deletedQuestionIds)
                {
                    var deleteParam = new[] { new SqlParameter("@id", SqlDbType.Int) { Value = id } };
                    await _sql.ExecuteNonQueryAsync(deleteQuery, conn, trans, deleteParam);
                }
            }

            if (answers.Count > 0)
            {
                foreach (var answer in answers)
                {
                    string query;
                    SqlParameter[] parameters;
                    if (answer.Id > 0)
                    {
                        // Cập nhật câu trả lời đã tồn tại
                        query = @"
                            UPDATE answer
                            SET Content = @content, IsCorrect = @is_correct, IsEnable = @is_enable
                            WHERE Id = @id;
                        ";
                        parameters = new[]
                        {
                            new SqlParameter("@id", SqlDbType.Int){ Value = answer.Id},
                            new SqlParameter("@content", SqlDbType.NVarChar){ Value = answer.Text},
                            new SqlParameter("@is_correct", SqlDbType.Bit){ Value = answer.IsCorrect},
                            new SqlParameter("@is_enable", SqlDbType.Int){ Value = 1}
                        };
                    }
                    else
                    {
                        // Thêm mới câu trả lời
                        query = @"
                            INSERT INTO answer (QuestionId, Content, IsCorrect, IsEnable)
                            VALUES (@question_id, @content, @is_correct, @is_enable);
                        ";
                        parameters = new[]
                        {
                            new SqlParameter("@question_id", SqlDbType.Int){ Value = questionId},
                            new SqlParameter("@content", SqlDbType.NVarChar){ Value = answer.Text},
                            new SqlParameter("@is_correct", SqlDbType.Bit){ Value = answer.IsCorrect},
                            new SqlParameter("@is_enable", SqlDbType.Int){ Value = 1}
                        };
                    }
                    await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
                }

            }

        }

        public async Task UpdateQuestionCategory(int questionId, List<int> categoryIds, SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                UPDATE QuestionCategory
                SET CategoryId = @category
                WHERE QuestionId = @question_id;
            ";

            var parameters = new[]
            {
                new SqlParameter("@question_id", SqlDbType.Int){ Value = questionId},
                new SqlParameter("@category", SqlDbType.Int){ Value = categoryIds.FirstOrDefault()}
            };
            await _sql.ExecuteNonQueryAsync(query, conn, trans, parameters);
        }
    }
}
