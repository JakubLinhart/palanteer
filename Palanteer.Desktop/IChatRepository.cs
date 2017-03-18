using System;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    public interface IChatRepository
    {
        event EventHandler<ChatLine> ChatLineAdded;

        Task SendChatLine(ChatLine line);
    }
}