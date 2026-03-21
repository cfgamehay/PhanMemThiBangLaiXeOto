namespace ApiThiBangLaiXeOto.DTOs
{
    public class SaveLearningProgressDto
    {
        public int QuestionId { get; set; }
        public int SelectedAnswerId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
