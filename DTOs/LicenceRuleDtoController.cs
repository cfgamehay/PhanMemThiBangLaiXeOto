using System.ComponentModel.DataAnnotations;

namespace ApiThiBangLaiXeOto.DTOs
{
    public class LicenceRuleDto
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int QuestionCount { get; set; }
    }
}