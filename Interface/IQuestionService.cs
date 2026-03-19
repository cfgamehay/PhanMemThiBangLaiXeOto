using ApiThiBangLaiXeOto.DTOs;
using Microsoft.Data.SqlClient;

namespace ApiThiBangLaiXeOto.Interface
{
    public interface IQuestionService
    {
        Task<int> InsertQuestion(QuestionCreateDTO dto, SqlConnection conn, SqlTransaction trans);
        Task InsertAnswers(int questionId, List<AnswerCreateDto> answers, SqlConnection conn, SqlTransaction trans);
        Task InsertQuestionCategory(int questionId, List<int> categoryIds, SqlConnection conn, SqlTransaction trans);
        Task UpdateQuestion(int id, QuestionUpdateDto dto, SqlConnection conn, SqlTransaction trans);
        Task UpdateAnswers(int questionId, List<AnswerUpdateDto> answers, List<int> deletedQuestionIds, SqlConnection conn, SqlTransaction trans);
        Task UpdateQuestionCategory(int questionId, List<int> categoryIds, SqlConnection conn, SqlTransaction trans);
    }
}
