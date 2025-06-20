using Microsoft.AspNetCore.SignalR;

namespace User.DTO
{
    public class ChatHub :Hub
    {
        public async Task NotifyNewOrder()
        {
            await Clients.All.SendAsync("ReceiveMessage", "✅ تم إضافة الطلب بنجاح!");
        }


    }
}
