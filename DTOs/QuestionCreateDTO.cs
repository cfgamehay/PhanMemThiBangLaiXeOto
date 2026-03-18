namespace ApiThiBangLaiXeOto.DTOs;

using System.ComponentModel.DataAnnotations;

public class QuestionCreateDTO
{
    [Required]
    [MinLength(10, ErrorMessage = "Câu hỏi phải có ít nhất 10 ký tự")]
    public string Question { get; set; } = null!;
    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 danh mục")]
    public List<int> CategoryIds { get; set; } = new();
    [Required]
    [MinLength(2, ErrorMessage = "Phải có ít nhất 2 đáp án")]
    public List<AnswerCreateDto> Answers { get; set; } = new();
    public string? Explain { get; set; }
    public string? ImageLink { get; set; }
    public IFormFile? Image { get; set; }

}