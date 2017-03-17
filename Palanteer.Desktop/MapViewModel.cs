using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    public sealed class MapViewModel : INotifyPropertyChanged
    {
        private PlayerMarker player;
        private PlayerMarker trackedPlayer;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PlayerMarker> Players { get; } = new ObservableCollection<PlayerMarker>();
        public ObservableCollection<PlaceMarker> Places { get; } = new ObservableCollection<PlaceMarker>();

        public PlayerMarker Player
        {
            get { return player; }
            set
            {
                if (player != null)
                {
                    Players.Remove(player);
                }

                player = value;

                Players.Add(player);

                OnPropertyChanged();
            }
        }

        public PlayerMarker TrackedPlayer
        {
            get { return trackedPlayer; }
            set
            {
                trackedPlayer = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
