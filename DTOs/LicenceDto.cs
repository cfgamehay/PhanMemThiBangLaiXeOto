using System.ComponentModel.DataAnnotations;

namespace ApiThiBangLaiXeOto.DTOs
{
    public class LicenceDto
    {
        [Required]
        public string LicenceCode { get; set; } = null!;
        public int QuestionCount { get; set; }
        public int Duration { get; set; }
        public int PassScore { get; set; }

    }
}