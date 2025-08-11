using CustomerSerrvices.ApplicationDbContext;
using freelancer.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class ChatHub : Hub
{
    private readonly DB _db;
    private static Dictionary<string, string> ConnectedUsers = new Dictionary<string, string>();
    public ChatHub(DB db)
    {
        _db = db;
    }

    private string GetUserId()
    {
        return Context.User?.FindFirstValue("ID")!;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
        {
            Context.Abort();
            return;
        }

        // انضمام المستخدم لجروب باسمه أو ID بتاعه
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }

    public async Task SendMessageToUser(string ReciverId, string message)
    {
        try
        {
            var senderId = GetUserId();

            if (senderId == null)
                return;

            if (string.IsNullOrWhiteSpace(ReciverId) || string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", "لا يمكن ارسال رسالة فارغة");
                return;
            }

            // 1. حفظ الرسالة
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = ReciverId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            _db.chatMessages.Add(chatMessage);

            // 2. تحديد الترتيب الموحد للمستخدمين لمنع تكرار السجل
            var user1 = string.Compare(senderId, ReciverId) < 0 ? senderId : ReciverId;
            var user2 = string.Compare(senderId, ReciverId) < 0 ? ReciverId : senderId;

            var conversation = await _db.chatSummaries
                .FirstOrDefaultAsync(c => c.User1Id == user1 && c.User2Id == user2);

            if (conversation == null)
            {
                // إنشاء ملخص جديد
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
                // تحديث الملخص
                conversation.LastMessage = message;
                conversation.LastMessageTime = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // 3. إرسال الرسالة للمستلم إن كان متصل
            if (ConnectedUsers.TryGetValue(ReciverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
            else
            {
                // المستقبل غير متصل، تنبيه للمرسل
                await Clients.Caller.SendAsync("Info", "المستقبل غير متصل حالياً.");
            }
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("Error", "حدث خطأ أثناء إرسال الرسالة.");
        }
    }

}
