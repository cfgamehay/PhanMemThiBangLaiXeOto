using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper
{
    public class QuestionMapper
    {
        public static QuestionRawDto ToRawQuestionListDto(SqlDataReader reader)
        {
            var dto = new QuestionRawDto
            {
                QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                QuestionContent = reader.GetString(reader.GetOrdinal("QuestionContent")),
                Explanation = reader.IsDBNull(reader.GetOrdinal("Explanation")) ? null : reader.GetString(reader.GetOrdinal("Explanation")),
                ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                AnswerId = reader.GetInt32(reader.GetOrdinal("AnswerId")),
                AnswerContent = reader.GetString(reader.GetOrdinal("AnswerContent")),
                IsCorrect = reader.GetBoolean(reader.GetOrdinal("IsCorrect")),
                IsCritical = reader.GetBoolean(reader.GetOrdinal("IsCritical"))
            };

            // Kiểm tra cột "Duration" có tồn tại hay không
            if (HasColumn(reader, "Duration"))
            {
                dto.Duration = reader.IsDBNull(reader.GetOrdinal("Duration")) ? 0 : reader.GetInt32(reader.GetOrdinal("Duration"));
            }

            // Tương tự cho LicenceCode nếu cần
            if (HasColumn(reader, "LicenceCode"))
            {
                dto.LicenceCode = reader.IsDBNull(reader.GetOrdinal("LicenceCode")) ? null : reader.GetString(reader.GetOrdinal("LicenceCode"));
            }

            return dto;
        }

        // Hàm bổ trợ để kiểm tra cột
        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }


    }
}

