using ApiThiBangLaiXeOto.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ApiThiBangLaiXeOto.Service
{
    public class ConsultationService
    {
        private readonly IHubContext<ConsultationHub> _hubContext;

        // 🔥 lưu timeout theo từng cặp call
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _timeouts
            = new ConcurrentDictionary<string, CancellationTokenSource>();

        public ConsultationService(IHubContext<ConsultationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // 🔑 tạo key duy nhất cho cuộc gọi
        private string GetKey(string a, string b)
        {
            return string.Compare(a, b) < 0
                ? $"{a}_{b}"
                : $"{b}_{a}";
        }

        public async Task HandleTimeout(string callerId, string targetId, string callId)
        {
            var key = GetKey(callerId, targetId);

            var cts = new CancellationTokenSource();
            _timeouts[key] = cts;

            try
            {
                // ✅ delay có thể bị huỷ
                await Task.Delay(15000, cts.Token);

                var caller = OnlineStore.Users.Values.FirstOrDefault(u => u.UserId == callerId);
                var target = OnlineStore.Users.Values.FirstOrDefault(u => u.UserId == targetId);
                if (caller == null || target == null ||
                    caller.CallId != callId || target.CallId != callId)
                {
                    return;
                }
                if (caller != null && target != null &&
                    caller.IsCalling && target.IsCalling &&
                    !(caller.IsInCall && target.IsInCall))
                {
                    caller.IsCalling = false;
                    target.IsCalling = false;

                    // broadcast lại danh sách
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
                        .SendAsync("CallTimeoutForReceiver");
                }
            }
            catch (TaskCanceledException)
            {
                // ✅ bị huỷ → không làm gì
                Console.WriteLine($"⛔ Timeout cancelled: {key}");
            }
            finally
            {
                _timeouts.TryRemove(key, out _);
            }
        }

        // 🔥 HÀM QUAN TRỌNG: huỷ timeout
        public void CancelTimeout(string callerId, string targetId)
        {
            var key = GetKey(callerId, targetId);

            if (_timeouts.TryRemove(key, out var cts))
            {
                cts.Cancel();
                Console.WriteLine($"✅ Cancel timeout: {key}");
            }
        }
    }
}