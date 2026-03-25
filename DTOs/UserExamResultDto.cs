namespace ApiThiBangLaiXeOto.DTOs
{
    public class UserExamResultDto
    {
        public int UserId { get; set; }
        public int? LicenceId { get; set; } = null;
        public int TotalCorrect { get; set; }
        public bool HitCritical { get; set; }
        public bool IsPassed { get; set; }
        public int TotalQuestion { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
