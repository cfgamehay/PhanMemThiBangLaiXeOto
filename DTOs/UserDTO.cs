namespace ApiThiBangLaiXeOto.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required DateTime CreatedAt { get; set; }
        public int Role { get; set; }
        public bool Status { get; set; }
    }
}
