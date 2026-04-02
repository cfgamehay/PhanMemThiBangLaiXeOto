namespace ApiThiBangLaiXeOto.DTOs
{
    public class OnlineUserDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } // "admin" | "user"
        public string ConnectionId { get; set; }
        public bool IsCalling { get; set; } = false;
    }
}
