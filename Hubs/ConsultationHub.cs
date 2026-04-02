using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Helper;
using ApiThiBangLaiXeOto.Service;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ApiThiBangLaiXeOto.Hubs
{
    public class ConsultationHub : Hub
    {
        private readonly SqlHelper _sql;
        public ConsultationHub(SqlHelper sql)
        {
            _sql = sql;
        }
        public async Task Register()
        {

            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
   
            if (userId == null || role == null) return;

            var userDb = await _sql.GetUserAsync(int.Parse(userId));

            var user = new OnlineUserDto
            {
                UserId = userId,
                Name = string.IsNullOrEmpty(userDb?.UserName)
                ? (role == "ADMIN" ? $"Admin {userId}" : $"User {userId}")
                : userDb.UserName,
                Role = role,
                ConnectionId = Context.ConnectionId
            };

            OnlineStore.Users[Context.ConnectionId] = user;

            await BroadcastUsers();
        }

        // 🔥 Tắt online
        public async Task SetOffline()
        {
            OnlineStore.Users.TryRemove(Context.ConnectionId, out _);
            await BroadcastUsers();
        }

        // 🔥 Trạng thái đang gọi
        public async Task SetCalling(bool isCalling)
        {
            if (OnlineStore.Users.TryGetValue(Context.ConnectionId, out var user))
            {
                user.IsCalling = isCalling;
                await BroadcastUsers();
            }
        }

        // 🔥 Auto offline khi đóng tab
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            OnlineStore.Users.TryRemove(Context.ConnectionId, out _);
            await BroadcastUsers();

            await base.OnDisconnectedAsync(exception);
        }

        // 🔥 Gửi danh sách (lọc role)
        private async Task BroadcastUsers()
        {
            var users = OnlineStore.Users.Values.ToList();

            foreach (var connection in OnlineStore.Users)
            {
                var currentUser = connection.Value;

                List<OnlineUserDto> filtered;

                if (currentUser.Role.ToUpper() == "ADMIN")
                {
                    filtered = users.Where(u => u.Role.ToUpper() == "USER").ToList();
                }
                else
                {
                    filtered = users.Where(u => u.Role.ToUpper() == "ADMIN").ToList();
                }

                await Clients.Client(connection.Key)
                    .SendAsync("ReceiveOnlineUsers", filtered);
            }
        }
    }
}