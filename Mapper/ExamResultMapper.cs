using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper
{
    public class ExamResultMapper
    {
        public static ExamResultDto ToExamResultDto(SqlDataReader reader)
        {
            return new ExamResultDto
            {
                TotalCorrect = reader.GetInt32(reader.GetOrdinal("TotalCorrect")),
                HitCritical = reader.GetInt32(reader.GetOrdinal("HitCritical")),
                TotalQuestion = reader.GetInt32(reader.GetOrdinal("TotalQuestion"))
            };
        }

        public static ExamHistoryDto ToExamHistoryDto(SqlDataReader reader)
        {
            return new ExamHistoryDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                TotalCorrect = reader.GetInt32(reader.GetOrdinal("TotalCorrect")),
                LicenceCode = reader.GetString(reader.GetOrdinal("LicenceCode")),
                isPassed = reader.GetBoolean(reader.GetOrdinal("isPassed")),
                HitCritical = reader.GetBoolean(reader.GetOrdinal("HitCritical")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        public static RawExamHistoryDetailDto ToRawExamHistoryDetail(SqlDataReader reader) {
            return new RawExamHistoryDetailDto
            {
                QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                QuestionContent = reader.GetString(reader.GetOrdinal("QuestionContent")),
                ImageLink = reader.IsDBNull(reader.GetOrdinal("ImageLink")) ? null : reader.GetString(reader.GetOrdinal("ImageLink")),
                IsCritical = reader.GetBoolean(reader.GetOrdinal("IsCritical")),
                Explain = reader.GetString(reader.GetOrdinal("Explain")),
                AnswerId = reader.GetInt32(reader.GetOrdinal("AnswerId")),
                AnswerContent = reader.GetString(reader.GetOrdinal("AnswerContent")),
                CorrectAnswer = reader.GetBoolean(reader.GetOrdinal("CorrectAnswer")),
                IsSelected = reader.GetBoolean(reader.GetOrdinal("IsSelected"))
            };
        }
    }
}
