namespace ApiThiBangLaiXeOto.DTOs
{
    public class RawLicenceListDto
    {

        public int Id { get; set; }
        public string LicenceCode { get; set; } = null!;
        public int TotalQuestion { get; set; }
        public int? QuestionCount { get; set; }
        public int Duration { get; set; }
        public int PassScore { get; set; }
        public string? CategoryName { get; set; }
        public int CategoryId { get; set; }
    }
}