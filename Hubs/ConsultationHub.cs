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

            
            var callId = Guid.NewGuid().ToString();
            // 🔥 set trạng thái
            caller.IsCalling = true;
            target.IsCalling = true;

            caller.CallId = callId;
            target.CallId = callId;

            // 🔥 gửi popup cho người bị gọi
            await Clients.Client(target.ConnectionId)
                .SendAsync("IncomingCall", new
                {
                    fromUserId = caller.UserId,
                    fromName = caller.Name
                });

            await BroadcastUsers();
            _ = _service.HandleTimeout(caller.UserId, target.UserId, callId);
        }
        public async Task AcceptCall(string targetUserId)
        {
            var caller = OnlineStore.Users.Values
                .FirstOrDefault(u => u.UserId == targetUserId);

            var current = OnlineStore.Users
                .FirstOrDefault(x => x.Key == Context.ConnectionId).Value;

            if (caller == null || current == null) return;

            caller.IsCalling = false;
            current.IsCalling = false;

            caller.IsInCall = true;
            current.IsInCall = true;


            _service.CancelTimeout(caller.UserId, current.UserId);

            var callAcceptedData = new
            {
                FromUserId = current.UserId,      // Người đang chấp nhận (current)
                FromName = current.Name,
                TargetUserId = caller.UserId,     // Đối phương
                TargetName = caller.Name
            };

            // Gửi cho người gọi (caller)
            await Clients.Client(caller.ConnectionId)
                .SendAsync("CallAccepted", callAcceptedData);

            // Gửi cho người nhận (current)
            await Clients.Client(current.ConnectionId)
                .SendAsync("CallAccepted", callAcceptedData);
        }

        public async Task RejectCall(string targetUserId)
        {
            var caller = OnlineStore.Users.Values.FirstOrDefault(u => u.UserId == targetUserId);
            OnlineStore.Users.TryGetValue(Context.ConnectionId, out var current);

            if (current != null)
            {
                current.IsCalling = false;
                current.IsInCall = false;
                current.CallId = null;
            }

            if (caller != null)
            {
                caller.IsCalling = false;
                caller.IsInCall = false;
                caller.CallId = null;
            }

            if (caller != null && current != null)
            {
                _service.CancelTimeout(caller.UserId, current.UserId);
            }

            await BroadcastUsers();

            if (caller != null)
            {
                await Clients.Client(caller.ConnectionId).SendAsync("CallRejected");
            }
        }
        // ================================
        // Huỷ cuộc gọi từ bên gọi (khi đang chờ accept)
        // ================================
        public async Task CancelCall()
        {
            var caller = OnlineStore.Users
                .FirstOrDefault(x => x.Key == Context.ConnectionId).Value;

            if (caller == null || !caller.IsCalling || string.IsNullOrEmpty(caller.CallId))
                return;

            var callId = caller.CallId;

            // Tìm người nhận
            var receiver = OnlineStore.Users.Values
                .FirstOrDefault(u => u.CallId == callId && u.ConnectionId != Context.ConnectionId);

            // Reset trạng thái cả hai bên
            caller.IsCalling = false;
            caller.CallId = null;

            if (receiver != null)
            {
                receiver.IsCalling = false;
                receiver.CallId = null;
                await Clients.Client(receiver.ConnectionId).SendAsync("CallRejected");
            }

            await BroadcastUsers();
        }
        // ================================
        // 📴 KẾT THÚC CUỘC GỌI (CẢ HAI BÊN ĐỀU GỌI ĐƯỢC)
        // ================================
        public async Task EndCall()
        {
            var currentConnectionId = Context.ConnectionId;

            // Tìm người đang gọi (current user)
            if (!OnlineStore.Users.TryGetValue(currentConnectionId, out var currentUser))
                return;

            // Nếu không đang trong cuộc gọi thì bỏ qua
            if (!currentUser.IsInCall || string.IsNullOrEmpty(currentUser.CallId))
            {
                await BroadcastUsers();
                return;
            }

            var callId = currentUser.CallId;

            // Tìm đối phương theo CallId
            var targetUser = OnlineStore.Users.Values
                .FirstOrDefault(u => u.CallId == callId && u.ConnectionId != currentConnectionId);

            // Reset trạng thái cho cả hai bên
            currentUser.IsCalling = false;
            currentUser.IsInCall = false;
            currentUser.CallId = null;

            if (targetUser != null)
            {
                targetUser.IsCalling = false;
                targetUser.IsInCall = false;
                targetUser.CallId = null;
            }

            // Gửi thông báo "CallEnded" cho cả hai bên (rất quan trọng)
            await Clients.Client(currentUser.ConnectionId).SendAsync("CallEnded");

            if (targetUser != null)
            {
                await Clients.Client(targetUser.ConnectionId).SendAsync("CallEnded");
            }

            // Broadcast lại danh sách online (cập nhật trạng thái IsCalling/IsInCall)
            await BroadcastUsers();

        }

        // ================================
        // 🔌 AUTO CLEANUP KHI USER DISCONNECT (F5, đóng tab, mất kết nối)
        // ================================
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            if (!OnlineStore.Users.TryGetValue(connectionId, out var user))
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            Console.WriteLine($"[OnDisconnectedAsync] User {user.Name} disconnected. ConnectionId: {connectionId}");

            // ==================== TRƯỜNG HỢP 1: ĐANG TRONG CUỘC GỌI ====================
            if (user.IsInCall && !string.IsNullOrEmpty(user.CallId))
            {
                var callId = user.CallId;

                // Tìm đối phương
                var otherUser = OnlineStore.Users.Values
                    .FirstOrDefault(u => u.CallId == callId && u.ConnectionId != connectionId);

                Console.WriteLine($"[OnDisconnectedAsync] Đang trong cuộc gọi {callId}. Reset cả hai bên.");

                // Reset trạng thái cho người vừa disconnect
                user.IsCalling = false;
                user.IsInCall = false;
                user.CallId = null;

                // Reset trạng thái cho đối phương
                if (otherUser != null)
                {
                    otherUser.IsCalling = false;
                    otherUser.IsInCall = false;
                    otherUser.CallId = null;

                    // Thông báo cho đối phương rằng cuộc gọi bị ngắt
                    await Clients.Client(otherUser.ConnectionId)
                        .SendAsync("CallEnded", "Đối phương đã ngắt kết nối đột ngột");
                }

                Console.WriteLine($"[OnDisconnectedAsync] Đã reset trạng thái cuộc gọi {callId}");
            }
            // ==================== TRƯỜNG HỢP 2: ĐANG CÓ INCOMING CALL (chưa accept) ====================
            else if (user.IsCalling && !string.IsNullOrEmpty(user.CallId))
            {
                var callId = user.CallId;

                Console.WriteLine($"[OnDisconnectedAsync] User {user.Name} có IncomingCall đang chờ → Reset và thông báo cho cả hai bên");

                // Tìm người gọi (caller)
                var caller = OnlineStore.Users.Values
                    .FirstOrDefault(u => u.CallId == callId && u.ConnectionId != connectionId);

                // Reset trạng thái cho cả hai bên
                user.IsCalling = false;
                user.CallId = null;

                if (caller != null)
                {
                    caller.IsCalling = false;
                    caller.CallId = null;

                    // Gửi CallRejected cho bên gọi
                    await Clients.Client(caller.ConnectionId).SendAsync("CallRejected");
                    Console.WriteLine($"[OnDisconnectedAsync] Đã gửi CallRejected cho Caller: {caller.Name}");
                }

                // Gửi CallRejected cho bên nhận (người vừa reload)
                await Clients.Client(user.ConnectionId).SendAsync("CallRejected");
                Console.WriteLine($"[OnDisconnectedAsync] Đã gửi CallRejected cho Receiver: {user.Name}");
            }

            // Xóa user khỏi danh sách online
            OnlineStore.Users.TryRemove(connectionId, out _);

            // Cập nhật danh sách online cho tất cả mọi người
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
        // ================================
        // 💬 CHAT REALTIME TRONG CUỘC GỌI
        // ================================

        // Bên gọi / nhận gửi tin nhắn
        public async Task SendMessage(string message)
        {
            var current = OnlineStore.Users
                .FirstOrDefault(x => x.Key == Context.ConnectionId).Value;

            if (current == null || !current.IsInCall || string.IsNullOrEmpty(current.CallId))
                return;

            var callId = current.CallId;

            // Tìm người còn lại trong cùng cuộc gọi
            var otherUser = OnlineStore.Users.Values
                .FirstOrDefault(u => u.CallId == callId && u.ConnectionId != Context.ConnectionId);

            if (otherUser == null) return;

            // Gửi cho người kia
            await Clients.Client(otherUser.ConnectionId)
                .SendAsync("ReceiveMessage", new ChatMessageDto
                {
                    Text = message,
                    FromUserId = current.UserId,
                    FromName = current.Name,
                    Timestamp = DateTime.UtcNow
                });

        }
        // ================================
        // 🎤 WEBRTC SIGNALING - VOICE CALL
        // ================================
        public async Task SendOffer(string targetUserId, string sdp)
        {
            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(sdp))
                return;

            var target = OnlineStore.Users.Values
            .FirstOrDefault(u => u.UserId == targetUserId);

            if (target == null)
            {
                Console.WriteLine($"❌ User {targetUserId} NOT FOUND");
                return;
            }

            var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Clients.Client(target.ConnectionId).SendAsync("ReceiveOffer", new
            {
                fromUserId,
                sdp
            });

            Console.WriteLine($"[WebRTC] Offer: {fromUserId} → {targetUserId}");
        }
        public async Task SendAnswer(string targetUserId, string sdp)
        {
            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(sdp))
                return;

            var target = OnlineStore.Users.Values
            .FirstOrDefault(u => u.UserId == targetUserId);

            if (target == null)
            {
                Console.WriteLine($"❌ User {targetUserId} NOT FOUND");
                return;
            }

            var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Clients.Client(target.ConnectionId).SendAsync("ReceiveAnswer", new
            {
                fromUserId,
                sdp
            });

            Console.WriteLine($"[WebRTC] Answer: {fromUserId} → {targetUserId}");
        }
        public async Task SendIceCandidate(
            string targetUserId,
            string candidate,
            string sdpMid,
            int sdpMLineIndex)
        {
            if (string.IsNullOrEmpty(targetUserId) || string.IsNullOrEmpty(candidate))
                return;

            var target = OnlineStore.Users.Values
            .FirstOrDefault(u => u.UserId == targetUserId);

            if (target == null)
            {
                Console.WriteLine($"❌ User {targetUserId} NOT FOUND");
                return;
            }

            var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await Clients.Client(target.ConnectionId).SendAsync("ReceiveIceCandidate", new
            {
                fromUserId,
                candidate,
                sdpMid,
                sdpMLineIndex
            });

            Console.WriteLine($"[WebRTC] ICE: {fromUserId} → {targetUserId}");
        }
    }
}