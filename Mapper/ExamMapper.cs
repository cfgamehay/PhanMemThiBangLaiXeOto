using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper
{
    public class ExamMapper
    {
        public static ExamRawDto ToRawQuestionListDto(SqlDataReader reader)
        {
            return new ExamRawDto
            {
                QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                QuestionContent = reader.GetString(reader.GetOrdinal("QuestionContent")),
                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                AnswerId = reader.GetInt32(reader.GetOrdinal("AnswerId")),
                AnswerContent = reader.GetString(reader.GetOrdinal("AnswerContent")),
                LicenceCode = reader.IsDBNull(reader.GetOrdinal("LicenceCode")) ? null : reader.GetString(reader.GetOrdinal("LicenceCode")),
                Duration = reader.IsDBNull(reader.GetOrdinal("Duration")) ? null : reader.GetInt32(reader.GetOrdinal("Duration"))
            };
        }
    }
}
