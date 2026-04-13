namespace ApiThiBangLaiXeOto.DTOs
{
    public class TrafficSignCreateDto
    {
        public string? ImageUrl { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}
