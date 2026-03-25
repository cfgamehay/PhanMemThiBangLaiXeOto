namespace ApiThiBangLaiXeOto.DTOs
{
    public class RawExamHistoryDetailDto
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string? ImageLink { get; set; }
        public bool IsCritical { get; set; }
        public string Explain { get; set; } = null!;
        public int AnswerId { get; set; }
        public string AnswerContent { get; set; } = null!;
        public bool CorrectAnswer { get; set; }
        public bool IsSelected { get; set; }

    }
}
