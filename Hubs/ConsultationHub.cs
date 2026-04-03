using ApiThiBangLaiXeOto.Data;
using ApiThiBangLaiXeOto.DTOs;
using ApiThiBangLaiXeOto.Service;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ApiThiBangLaiXeOto.Hubs
{
    public class ConsultationHub : Hub
    {
        private readonly SqlHelper _sql;
        private readonly ConsultationService _service;

        public ConsultationHub(SqlHelper sql, ConsultationService service)
        {
            _sql = sql;
            _service = service;
        }

        // ================================
        // 🟢 ONLINE
        // ================================
        public async Task Register()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                return;

            var userDb = await _sql.GetUserAsync(int.Parse(userId));

            var user = new OnlineUserDto
            {
                UserId = userId,
                Name = string.IsNullOrEmpty(userDb?.UserName)
                    ? (role.ToUpper() == "ADMIN" ? $"Admin {userId}" : $"User {userId}")
                    : userDb.UserName,
                Role = role.ToUpper(),
                ConnectionId = Context.ConnectionId,
                IsCalling = false
            };

            OnlineStore.Users[Context.ConnectionId] = user;

            await BroadcastUsers();
        }

        // ================================
        // 🔴 OFFLINE
        // ================================
        public async Task SetOffline()
        {
            OnlineStore.Users.TryRemove(Context.ConnectionId, out _);
            await BroadcastUsers();
        }

        // ================================
        // 📞 GỌI NGƯỜI KHÁC
        // ================================
        public async Task CallUser(string targetUserId)
        {
            var caller = OnlineStore.Users
                .FirstOrDefault(x => x.Key == Context.ConnectionId).Value;

            if (caller == null) return;

            var target = OnlineStore.Users.Values
                .FirstOrDefault(u => u.UserId == targetUserId);

            if (target == null) return;

            if (caller.IsCalling || target.IsCalling)
                return;

            // 🔥 set trạng thái
            caller.IsCalling = true;
            target.IsCalling = true;

            // 🔥 gửi popup cho người bị gọi
            await Clients.Client(target.ConnectionId)
                .SendAsync("IncomingCall", new
                {
                    fromUserId = caller.UserId,
                    fromName = caller.Name
                });

            await BroadcastUsers();
            _ = _service.HandleTimeout(caller.UserId, target.UserId);
        }
        public async Task AcceptCall(string targetUserId)
        {
            var caller = OnlineStore.Users.Values
                .FirstOrDefault(u => u.UserId == targetUserId);

            var current = OnlineStore.Users
                .FirstOrDefault(x => x.Key == Context.ConnectionId).Value;

            if (caller == null || current == null) return;

            // 🔥 vẫn giữ trạng thái calling (đang trong call)

            await Clients.Client(caller.ConnectionId)
                .SendAsync("CallAccepted");

            await Clients.Client(current.ConnectionId)
                .SendAsync("CallAccepted");
        }

        public async Task RejectCall(string targetUserId)
        {
            var caller = OnlineStore.Users.Values
                .FirstOrDefault(u => u.UserId == targetUserId);

            OnlineStore.Users.TryGetValue(Context.ConnectionId, out var current);

            if (current != null)
                current.IsCalling = false;

            if (caller != null)
                caller.IsCalling = false;

            await BroadcastUsers();

            if (caller != null)
            {
                await Clients.Client(caller.ConnectionId)
                    .SendAsync("CallRejected");
            }
        }
        // ================================
        // 📴 KẾT THÚC GỌI
        // ================================
        public async Task EndCall(string targetUserId)
        {
            var caller = OnlineStore.Users
                .FirstOrDefault(x => x.Key == Context.ConnectionId).Value;

            var target = OnlineStore.Users.Values
                .FirstOrDefault(u => u.UserId == targetUserId);

            if (caller != null)
                caller.IsCalling = false;

            if (target != null)
                target.IsCalling = false;

            await BroadcastUsers();
        }

        // ================================
        // 🔌 AUTO OFFLINE (đóng tab)
        // ================================
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            OnlineStore.Users.TryRemove(Context.ConnectionId, out _);
            await BroadcastUsers();

            await base.OnDisconnectedAsync(exception);
        }

        // ================================
        // 📡 BROADCAST REALTIME
        // ================================
        private async Task BroadcastUsers()
        {
            var users = OnlineStore.Users.Values.ToList();

            foreach (var connection in OnlineStore.Users)
            {
                var currentUser = connection.Value;

                List<OnlineUserDto> filtered;

                if (currentUser.Role == "ADMIN")
                {
                    filtered = users.Where(u => u.Role == "USER").ToList();
                }
                else
                {
                    filtered = users.Where(u => u.Role == "ADMIN").ToList();
                }

                await Clients.Client(connection.Key)
                    .SendAsync("ReceiveOnlineUsers", filtered, currentUser.IsCalling);
            }
        }
    }
}