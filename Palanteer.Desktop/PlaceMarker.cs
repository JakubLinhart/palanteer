using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Palanteer.Desktop
{
    public sealed class PlaceMarker : IMarker
    {
        public PlaceMarker(Place place)
        {
            Place = place;
            Name = place.Name;
            X = place.X;
            Y = place.Y;
        }

        public Place Place { get; }

        public int X
        {
            get { return Place.X; }
            set
            {
                Place.X = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Y
        {
            get { return Place.Y; }
            set
            {
                Place.Y = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return Place.Name; }
            set
            {
                Place.Name = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get { return Place.Type; }
            set
            {
                Place.Type = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return Place.Description; }
            set
            {
                Place.Description = value;
                OnPropertyChanged();
            }
        }

        public void Select()
        {
            OnSelected();
        }

        public event EventHandler Selected;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }
    }
}