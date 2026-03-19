using System.ComponentModel.DataAnnotations;

namespace ApiThiBangLaiXeOto.DTOs
{
    public class AnswerUpdateDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
