using ApiThiBangLaiXeOto.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ApiThiBangLaiXeOto.Service
{
    public class ConsultationService
    {
        private readonly IHubContext<ConsultationHub> _hubContext;

        public ConsultationService(IHubContext<ConsultationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task HandleTimeout(string callerId, string targetId)
        {
            await Task.Delay(15000);

            var caller = OnlineStore.Users.Values
                .FirstOrDefault(u => u.UserId == callerId);

            var target = OnlineStore.Users.Values
                .FirstOrDefault(u => u.UserId == targetId);

            if (caller != null && target != null &&
                caller.IsCalling && target.IsCalling)
            {
                caller.IsCalling = false;
                target.IsCalling = false;

                // broadcast lại list
                var users = OnlineStore.Users.Values.ToList();

                foreach (var conn in OnlineStore.Users)
                {
                    var current = conn.Value;

                    var filtered = current.Role == "ADMIN"
                        ? users.Where(u => u.Role == "USER").ToList()
                        : users.Where(u => u.Role == "ADMIN").ToList();

                    await _hubContext.Clients.Client(conn.Key)
                        .SendAsync("ReceiveOnlineUsers", filtered, current.IsCalling);
                }

                // 🔥 gửi timeout
                await _hubContext.Clients.Client(caller.ConnectionId)
                    .SendAsync("CallTimeout");

                await _hubContext.Clients.Client(target.ConnectionId)
                    .SendAsync("CallTimeout");
            }
        }
    }
}
