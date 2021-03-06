﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    internal sealed class ChatRepository : IChatRepository
    {
        private readonly HttpClient httpClient;

        public ChatRepository(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public event EventHandler<ChatLine> ChatLineAdded;

        public Task SendChatLine(ChatLine line) => httpClient.PostAsJsonAsync("api/chat", line);

        public void OnChatLineAdded(ChatLine line)
        {
            ChatLineAdded?.Invoke(this, line);
        }

        public async Task<IEnumerable<ChatLine>> GetHistory()
        {
            var response = await httpClient.GetAsync("api/chat");
            return await response.Content.ReadAsAsync<IEnumerable<ChatLine>>();

        }
    }
}