namespace ApiThiBangLaiXeOto.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string? Explanation { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsCritical { get; set; }
        public List<int> Categories { get; set; } = new();
        public List<AnswerDto> Answers { get; set; } = new();
    }
}
