using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Palanteer.Desktop
{
    public sealed class MapViewModel : INotifyPropertyChanged
    {
        private PlayerMarker player;
        private PlayerMarker trackedPlayer;
        private Point selectedPosition;
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

        public Point SelectedPosition
        {
            get { return selectedPosition; }
            set
            {
                selectedPosition = value; 
                OnPropertyChanged();
            }
        }

        public PlayerMarker TrackedPlayer
        {
            get { return trackedPlayer; }
            set
            {
                if (trackedPlayer != null)
                    trackedPlayer.PropertyChanged -= OnPlayerPropertyChanged;

                trackedPlayer = value;
                if (trackedPlayer != null)
                {
                    trackedPlayer.PropertyChanged += OnPlayerPropertyChanged;
                    SelectedPosition = new Point(trackedPlayer.X, trackedPlayer.Y);
                }
                OnPropertyChanged();
            }
        }

        private void OnPlayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SelectedPosition = new Point(TrackedPlayer.X, trackedPlayer.Y);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
