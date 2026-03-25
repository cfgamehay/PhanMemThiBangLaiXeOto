namespace ApiThiBangLaiXeOto.DTOs
{
    public class ExamDto
    {
        public int Id { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public bool IsCritical { get; set; }
        public List<ExamAnswerDto> Answers { get; set; } = new();
    }
}
