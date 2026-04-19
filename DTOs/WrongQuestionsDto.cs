namespace ApiThiBangLaiXeOto.DTOs
{
    public class WrongQuestionsDto
    {
        public int QuestionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int WrongCount { get; set; }
    }
}
