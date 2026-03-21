using System.ComponentModel.DataAnnotations;

namespace ApiThiBangLaiXeOto.DTOs
{
    public class LicenceCreateDto
    {
        [Required]
        public string LicenceCode { get; set; } = null!;
        [Required]
        public int QuestionCount { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        public int PassScore { get; set; }
        [Required]
        public List<LicenceRuleCreateDto> LicenceRule { get; set; } = new();
    }
}