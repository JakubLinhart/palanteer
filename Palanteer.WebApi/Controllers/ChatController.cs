using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.SignalR;

namespace Palanteer.WebApi.Controllers
{
    public class ChatController : ApiController
    {
        private static readonly List<ChatLine> Lines = new List<ChatLine>();

        public IEnumerable<ChatLine> Get() => Lines;

        public void Post([FromBody]ChatLine line)
        {
            Lines.Add(line);

            var context = GlobalHost.ConnectionManager.GetHubContext<PalanteerHub>();

            context.Clients.All.ChatLineAdded(line);
        }
    }
}