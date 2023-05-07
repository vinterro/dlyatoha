using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Thread_.NET.Common.DTO.Post;

namespace Thread_.NET.BLL.Hubs
{
    public sealed class PostHub : Hub
    {
        public async Task Send(PostDTO post)
        {
            await Clients.All.SendAsync("NewPost", post);
        }


        public async Task SomeMethod()
        {
            // Получение айди клиента хаба
            string connectionId = Context.ConnectionId;

            // Используйте айди клиента хаба для отправки сообщений только этому клиенту
            await Clients.Client(connectionId).SendAsync("SomeMethodResponse");
        }

        public async Task AddUserToGroup(int userId)
        {
            // Добавление пользователя в группу
            await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        }
        public async Task DeleteUserToGroup(int userId)
        {
            // Добавление пользователя в группу
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        }
    }
}
