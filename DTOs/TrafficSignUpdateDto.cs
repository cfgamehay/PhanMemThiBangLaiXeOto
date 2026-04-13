namespace ApiThiBangLaiXeOto.DTOs
{
    public class TrafficSignUpdateDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }

        public IFormFile? Image { get; set; }

        public bool RemoveImage { get; set; }
    }
}