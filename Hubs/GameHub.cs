using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace quizter_be.Hubs
{
    public class GameHub : Hub
    {
        public async Task GetQuestion()
        {
            await Clients.All.SendAsync("NextQuestion");
        }
    }
}