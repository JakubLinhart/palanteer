using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace Palanteer.WebApi.Controllers
{
    public class PlacesController : ApiController
    {
        private static readonly Dictionary<string, Place> Places = new Dictionary<string, Place>();

        public IEnumerable<Place> Get() => Places.Values;

        public Place Get(string id) => Places[id];

        public void Post([FromBody]Place place)
        {
            Places[place.Id] = place;
            Thread.Sleep(5000);
        }

        public void Delete(string id)
        {
            Places.Remove(id);
        }
    }
}