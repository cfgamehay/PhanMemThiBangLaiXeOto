using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper
{
    public class QuestionMapper
    {
        public static QuestionRawDto ToRawQuestionListDto(SqlDataReader reader)
        {
            return new QuestionRawDto
            {
                QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                QuestionContent = reader.GetString(reader.GetOrdinal("QuestionContent")),
                Explanation = reader.GetString(reader.GetOrdinal("Explanation")),
                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                AnswerId = reader.GetInt32(reader.GetOrdinal("AnswerId")),
                AnswerContent = reader.GetString(reader.GetOrdinal("AnswerContent")),
                IsCorrect = reader.GetBoolean(reader.GetOrdinal("IsCorrect")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"))
            
            };
        }

        
    }
}

