using System.ComponentModel.DataAnnotations;

namespace ApiThiBangLaiXeOto.DTOs
{
    public class LicenceRuleCreateDto
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int QuestionCount { get; set; }
    }
}