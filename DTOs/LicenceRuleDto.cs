using System.ComponentModel.DataAnnotations;

namespace ApiThiBangLaiXeOto.DTOs
{
    public class LicenceRuleDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int QuestionCount { get; set; }
    }
}