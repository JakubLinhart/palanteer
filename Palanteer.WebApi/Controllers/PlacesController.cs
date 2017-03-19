using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Palanteer.WebApi.Models;

namespace Palanteer.WebApi.Controllers
{
    public class PlacesController : ApiController
    {
        static PlacesController()
        {
            Repository.Connect();
        }

        private static readonly PlacesRepository Repository = new PlacesRepository();

        private static readonly Lazy<Dictionary<string, Place>> Places = new Lazy<Dictionary<string, Place>>(LoadPlaces);

        private static Dictionary<string, Place> LoadPlaces() =>
            Repository.Get().ToDictionary(p => p.Id);

        public IEnumerable<Place> Get() => Places.Value.Values;

        public Place Get(string id) => Places.Value[id];

        public async Task Post([FromBody]Place place)
        {
            await Repository.Update(place);

            Places.Value[place.Id] = place;
        }

        public async Task Delete(string id)
        {
            await Repository.Delete(id);

            Places.Value.Remove(id);
        }
    }
}