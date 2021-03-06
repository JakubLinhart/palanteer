﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    internal class PlayerRepository : IPlayerRepository 
    {
        private readonly HttpClient httpClient;

        public PlayerRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Player[]> GetAll()
        {
            var response = await httpClient.GetAsync("api/players");
            return await response.Content.ReadAsAsync<Player[]>();
        }

        public async Task Update(Player player)
        {
            if (httpClient.BaseAddress != null)
                await httpClient.PostAsJsonAsync("api/players", player);
        }

        public async Task Delete(string playerId)
        {
            if (httpClient.BaseAddress != null)
                await httpClient.DeleteAsync($"api/players/{playerId}");
        }
    }
}
