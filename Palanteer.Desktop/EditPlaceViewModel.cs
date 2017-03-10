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
        private readonly ObservableCollection<Marker> placesCollection;
        private PlaceMarker selectedPlace;

        public EditPlaceViewModel(Marker player, ObservableCollection<Marker> placesCollection,
            IPlaceRepository placeRepository)
        {
            this.placesCollection = placesCollection;
            this.placeRepository = placeRepository;
            Player = player;
        }

        public Marker Player { get; }

        public PlaceMarker SelectedPlace
        {
            get { return selectedPlace; }
            set
            {
                if (selectedPlace != null)
                    selectedPlace.PropertyChanged -= OnPlacePropertyChanged;

                selectedPlace = value;

                if (selectedPlace != null)
                    selectedPlace.PropertyChanged += OnPlacePropertyChanged;

                OnPropertyChanged();
                // ReSharper disable ExplicitCallerInfoArgument
                OnPropertyChanged("CanEdit");
                OnPropertyChanged("SelectedPlace");
                // ReSharper restore ExplicitCallerInfoArgument
            }
        }

        private async void OnPlacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await UpdatePlace();
        }

        private readonly IPlaceRepository placeRepository;

        public bool CanEdit => SelectedPlace?.CanEdit ?? false;

        public async Task UpdatePlace()
        {
            selectedPlace.Place.X = selectedPlace.X;
            selectedPlace.Place.Y = selectedPlace.Y;
            selectedPlace.Place.Name = selectedPlace.Name;

            await placeRepository.Update(selectedPlace.Place);
        }

        public async Task NewPlace()
        {
            var place = new Place() {Id = IdGenerator.Generate(), Name = "new place", X = Player.X, Y = Player.Y};

            SelectedPlace = CreatePlaceMarker(place);

            await placeRepository.Create(place);
        }

        private PlaceMarker CreatePlaceMarker(Place place)
        {
            var marker = new PlaceMarker(place);
            marker.Selected += OnPlaceSelected;
            placesCollection.Add(marker);

            return marker;
        }

        public void AddPlace(Place place)
        {
            CreatePlaceMarker(place);
        }

        private void OnPlaceSelected(object sender, EventArgs e)
        {
            if (sender is PlaceMarker marker)
            {
                SelectedPlace = marker;
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
                placeRepository.Delete(SelectedPlace.Place);
                SelectedPlace = null;
            }
        }
    }
}
