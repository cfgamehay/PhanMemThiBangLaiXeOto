namespace ApiThiBangLaiXeOto.DTOs
{
    public class ExamHistoryDetailDto
    {
        public int QuestionId { get; set; }
        public string QuestionContent { get; set; } = null!;
        public string ImageLink { get; set; } = null!;
        public bool IsCritical { get; set; }
        public string Explain { get; set; } = null!;
        public List<ExamHistoryAnswerDto> Answers { get; set; } = new();
    }
}
