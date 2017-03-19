using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Palanteer.WebApi.Models;

namespace Palanteer.WebApi.Controllers
{
    public class ChatController : ApiController
    {
        private static readonly ChatRepository Repository;

        static ChatController()
        {
            Repository = new ChatRepository();
            Repository.Connect();
        }

        private static readonly Lazy<List<ChatLine>> Lines = new Lazy<List<ChatLine>>(LoadChatHistory);
        
        private static List<ChatLine> LoadChatHistory() =>
            Repository.GetAfter(DateTime.UtcNow.AddDays(-5)).OrderBy(x => x.Timestamp).ToList();

        public IEnumerable<ChatLine> Get() => Lines.Value;

        public async Task Post([FromBody]ChatLine line)
        {
            await Repository.Insert(line);
            
            Lines.Value.Add(line);

            var context = GlobalHost.ConnectionManager.GetHubContext<PalanteerHub>();

            context.Clients.All.ChatLineAdded(line);
        }
    }
}