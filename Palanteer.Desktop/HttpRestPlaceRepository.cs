using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Palanteer.Desktop
{
    internal sealed class HttpRestPlaceRepository : IPlaceRepository
    {
        private readonly HttpClient httpClient;

        public HttpRestPlaceRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public Task Create(Place place) => httpClient.PostAsJsonAsync("api/places", place);

        public Task Update(Place place) => httpClient.PostAsJsonAsync("api/places", place);

        public Task Delete(Place place) => httpClient.DeleteAsync($"api/places/{place.Id}");

        public async Task<IEnumerable<Place>> Get()
        {
            var response = await httpClient.GetAsync("api/places");
            return await response.Content.ReadAsAsync<IEnumerable<Place>>();
        }
    }
}
