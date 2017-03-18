using System;

namespace Palanteer
{
    public sealed class ChatLine
    {
        public string Name { get; set; }

        public string Message { get; set; }

        public DateTime? Timestamp { get; set; }

        public string Text => Timestamp.HasValue ? $"{Timestamp:t} {Name}: {Message}" : $"{Name}: {Message}";
    }
}