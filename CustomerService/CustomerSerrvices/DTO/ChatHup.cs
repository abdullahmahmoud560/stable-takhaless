using CustomerSerrvices.ApplicationDbContext;
using freelancer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class ChatHub : Hub
{
    private readonly DB _db;
    private static readonly Dictionary<string, string> ConnectedUsers = new();

    public ChatHub(DB db)
    {
        _db = db;
    }

    private string? GetUserId()
    {
        return Context.User?.FindFirstValue("ID");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            Context.Abort(); // لو مفيش ID في التوكن
            return;
        }

        // سجل الاتصال
        ConnectedUsers[userId] = Context.ConnectionId;

        // انضمام المستخدم لجروب خاص بيه
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await Clients.Caller.SendAsync("SystemMessage", "✅ Connected to ChatHub");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            ConnectedUsers.Remove(userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    
    public async Task SendMessageToUser(string receiverId, string message)
    {
          var senderId = GetUserId();
            if (string.IsNullOrEmpty(senderId))
            {
                await Clients.Caller.SendAsync("Error", "🚫 المستخدم غير معرّف");
                return;
            }

            if (string.IsNullOrWhiteSpace(receiverId) || string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", "⚠️ لا يمكن ارسال رسالة فارغة");
                return;
            }

            // 1. حفظ الرسالة
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            _db.chatMessages.Add(chatMessage);

            // 2. تحديث ملخص المحادثة
            var user1 = string.Compare(senderId, receiverId, StringComparison.Ordinal) < 0 ? senderId : receiverId;
            var user2 = string.Compare(senderId, receiverId, StringComparison.Ordinal) < 0 ? receiverId : senderId;

            var conversation = await _db.chatSummaries
                .FirstOrDefaultAsync(c => c.User1Id == user1 && c.User2Id == user2);

            if (conversation == null)
            {
                conversation = new ChatSummary
                {
                    User1Id = user1,
                    User2Id = user2,
                    LastMessage = message,
                    LastMessageTime = DateTime.UtcNow,
                };
                _db.chatSummaries.Add(conversation);
            }
            else
            {
                conversation.LastMessage = message;
                conversation.LastMessageTime = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // 3. إرسال الرسالة للمستلم (لو متصل)
            if (ConnectedUsers.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", new
                {
                    senderId,
                    receiverId,
                    message,
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                await Clients.Caller.SendAsync("Info", "📭 المستقبل غير متصل حالياً.");
            }

    }
}
