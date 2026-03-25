namespace ApiThiBangLaiXeOto.DTOs
{
    public class ExamRawDto
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public int AnswerId { get; set; }
        public string AnswerContent { get; set; } = null!;
        public string? LicenceCode { get; set; }
        public int? Duration { get; set; }

    }
}
