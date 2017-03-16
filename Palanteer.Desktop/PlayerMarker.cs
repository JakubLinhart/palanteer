using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Palanteer.Desktop
{
    public sealed class PlayerMarker : IMarker
    {
        public PlayerMarker(Player player)
        {
            Model = player;
        }

        public int X
        {
            get { return Model.X; }
            set
            {
                Model.X = value;
                OnPropertyChanged();
            }
        }

        public int Y
        {
            get { return Model.Y; }
            set
            {
                Model.Y = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                Model.Name = value;
                OnPropertyChanged();
            }
        }

        public string Id => Model.Id;

        public Player Model { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateFromModel(Player player)
        {
            X = player.X;
            Y = player.Y;
            Name = player.Name;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}