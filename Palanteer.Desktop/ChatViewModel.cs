using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Palanteer.Desktop
{
    internal sealed class ChatViewModel : INotifyPropertyChanged
    {
        private readonly PlayerMarker player;
        private readonly IChatRepository chatRepository;

        private string prompt;

        public ChatViewModel(PlayerMarker player, IChatRepository chatRepository, Dispatcher dispatcher)
        {
            this.player = player;
            this.chatRepository = chatRepository;
            this.chatRepository.ChatLineAdded += (sender, line) => dispatcher.Invoke(() => Lines.Add(line));
        }

        public string Prompt
        {
            get { return prompt; }
            set
            {
                prompt = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ChatLine> Lines { get; } = new ObservableCollection<ChatLine>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void EnterLine()
        {
            chatRepository.SendChatLine(new ChatLine { Message = Prompt, Name = this.player.Name, Timestamp = DateTime.Now});
            Prompt = string.Empty;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}