using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Palanteer.Desktop
{
    internal sealed class ChatViewModel : INotifyPropertyChanged
    {
        private readonly PlayerMarker player;
        private readonly IChatRepository chatRepository;
        private readonly Dispatcher dispatcher;

        private string prompt;
        private ObservableCollection<ChatLine> lines = new ObservableCollection<ChatLine>();

        public ChatViewModel(PlayerMarker player, IChatRepository chatRepository, Dispatcher dispatcher)
        {
            this.player = player;
            this.chatRepository = chatRepository;
            this.dispatcher = dispatcher;
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

        public ObservableCollection<ChatLine> Lines
        {
            get { return lines; }
            set
            {
                lines = value;
                OnPropertyChanged();
            }
        }

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