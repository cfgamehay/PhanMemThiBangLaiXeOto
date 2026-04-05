namespace ApiThiBangLaiXeOto.DTOs
{
    public class ChatMessageDto
    {
        public string Text { get; set; } = string.Empty;
        public string FromUserId { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
