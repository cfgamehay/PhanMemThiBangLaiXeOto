namespace ApiThiBangLaiXeOto.DTOs
{
    public class AnswerDto
    {
        public int Id { get; set; }
        public string AnswerContent { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}