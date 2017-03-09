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
    public class EditPlaceViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Place> placesCollection;
        private Place selectedPlace;

        public EditPlaceViewModel(Place player, ObservableCollection<Place> placesCollection)
        {
            this.placesCollection = placesCollection;
            Player = player;
        }

        public Place Player { get; }

        public Place SelectedPlace
        {
            get { return selectedPlace; }
            set
            {
                selectedPlace = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("CanEdit");
            }
        }

        public bool CanEdit => SelectedPlace?.CanEdit ?? false;

        public void NewPlace()
        {
            SelectedPlace = new Place() {Name = "new place", X = Player.X, Y = Player.Y};
            SelectedPlace.Selected += OnPlaceSelected;
            placesCollection.Add(SelectedPlace);
        }

        private void OnPlaceSelected(object sender, EventArgs e)
        {
            if (sender is Place place)
            {
                SelectedPlace = place;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void DeletePlace()
        {
            if (SelectedPlace != null)
            {
                placesCollection.Remove(SelectedPlace);
                SelectedPlace = null;
            }
        }
    }
}
