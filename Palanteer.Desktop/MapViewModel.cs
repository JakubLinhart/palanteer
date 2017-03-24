using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Palanteer.Desktop
{
    public sealed class MapViewModel : INotifyPropertyChanged
    {
        private string palanteerUrl = "http://palanteer.azurewebsites.net/";
        private PlayerMarker player;
        private Point selectedPosition;
        private PlayerMarker trackedPlayer;

        public ObservableCollection<PlayerMarker> Players { get; } = new ObservableCollection<PlayerMarker>();
        public ObservableCollection<PlaceMarker> Places { get; } = new ObservableCollection<PlaceMarker>();

        public PlayerMarker Player
        {
            get { return player; }
            set
            {
                if (player != null)
                    Players.Remove(player);

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

        public string PalanteerUrl
        {
            get { return palanteerUrl; }
            set
            {
                palanteerUrl = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

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