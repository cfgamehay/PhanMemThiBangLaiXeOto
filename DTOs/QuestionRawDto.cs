namespace ApiThiBangLaiXeOto.DTOs
{
    public class QuestionRawDto
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string Explanation { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public int AnswerId { get; set; }
        public string AnswerContent { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public bool IsCritical { get; set; }
        public int CategoryId { get; set; }

    }
}
