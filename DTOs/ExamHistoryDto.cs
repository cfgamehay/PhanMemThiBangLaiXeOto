namespace ApiThiBangLaiXeOto.DTOs
{
    public class ExamHistoryDto
    {
        public int Id { get; set; }
        public int TotalCorrect { get; set; }
        public string LicenceCode { get; set; } = string.Empty;
        public bool isPassed { get; set; }
        public bool HitCritical { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
