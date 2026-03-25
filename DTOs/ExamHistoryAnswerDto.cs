namespace ApiThiBangLaiXeOto.DTOs
{
    public class ExamHistoryAnswerDto
    {
        public int AnswerId { get; set; }
        public string AnswerContent { get; set; } = null!;
        public bool CorrectAnswer { get; set; }
        public bool IsSelected { get; set; }
    }
}
