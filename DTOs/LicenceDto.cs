using System.ComponentModel.DataAnnotations;

namespace ApiThiBangLaiXeOto.DTOs
{
    public class LicenceDto
    {
        [Required]
        public int Id { get; set; } 
        public string LicenceCode { get; set; } = null!;
        public int TotalQuestion { get; set; }
        public int Duration { get; set; }
        public int PassScore { get; set; }
        public List<LicenceRuleDto> LicenceRule { get; set; } = new();

    }
}