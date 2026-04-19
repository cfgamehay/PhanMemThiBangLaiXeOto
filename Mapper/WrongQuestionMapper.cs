using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Mapper;

    public class WrongQuestionMapper
    {
        public static WrongQuestionsDto ToWrongQuestionsDto(SqlDataReader reader)
        {
            return new WrongQuestionsDto
            {
                QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                Content = reader["Content"].ToString() ?? string.Empty,
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader["CategoryName"].ToString() ?? string.Empty,
                WrongCount = reader.GetInt32(reader.GetOrdinal("WrongCount"))
            };
        }
    }

