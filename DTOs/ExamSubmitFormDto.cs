namespace ApiThiBangLaiXeOto.DTOs
{
    public class ExamSubmitFormDto
    {
        public int? LicenceId { get; set; }
        public List<ExamAnswerSubmitDto> Answers { get; set; } = new();
    }
}
