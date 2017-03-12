using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.SignalR;

namespace Palanteer.WebApi.Controllers
{
    public class PlayersController : ApiController
    {
        private static readonly Dictionary<string, Player> Players = new Dictionary<string, Player>();

        public IEnumerable<Player> Get()
        {
            return Players.Values;
        }

        public void Post([FromBody]Player player)
        {
            Players[player.Id] = player;

            var context = GlobalHost.ConnectionManager.GetHubContext<PalanteerHub>();
            context.Clients.All.PlayerUpdated(player);
        }

        public void Delete(string id)
        {
            Players.Remove(id);

            var context = GlobalHost.ConnectionManager.GetHubContext<PalanteerHub>();
            context.Clients.All.PlayerRemoved(id);
        }
    }
}