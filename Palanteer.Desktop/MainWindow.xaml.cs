using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Ultima;

namespace Palanteer.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer playerPositionRefreshTimer = new DispatcherTimer();
        private readonly Marker player = new Marker(null, canEdit: false) {Name = "Player"};

        public MainWindow()
        {
            InitializeComponent();

            _mapControl.MarkersCollection.Add(player);

            Task.Run(() => InitializePlayerPositionTracking());
            InitializeEditPlaceViewModel();
        }

        private static readonly HttpClient Client = new HttpClient();

        private void InitializeEditPlaceViewModel()
        {
            Client.BaseAddress = new Uri("http://localhost:2044/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var placeRepository = new HttpRestPlaceRepository(Client);
            var placeViewModel = new EditPlaceViewModel(player, _mapControl.MarkersCollection, placeRepository);
            _placeControl.DataContext = placeViewModel;

            Task.Run(() => LoadSharedPlaces(placeViewModel, placeRepository));
        }

        private async Task LoadSharedPlaces(EditPlaceViewModel placeViewModel, IPlaceRepository placeRepository)
        {
            var places = await placeRepository.Get();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var place in places)
                {
                    placeViewModel.AddPlace(place);
                }
            });
        }

        private void InitializePlayerPositionTracking()
        {
            Ultima.Client.Calibrate();

            playerPositionRefreshTimer.Tick += PlayerPositionRefreshTimerOnTick;
            playerPositionRefreshTimer.Interval = TimeSpan.FromSeconds(1);
            playerPositionRefreshTimer.Start();
        }

        private void PlayerPositionRefreshTimerOnTick(object sender, EventArgs eventArgs)
        {
            RefreshPlayerPosition();
        }

        private void RefreshPlayerPosition()
        {
            int x = 0, y = 0, z = 0, facet = 0;
            if (Ultima.Client.FindLocation(ref x, ref y, ref z, ref facet))
            {
                player.X = x;
                player.Y = y;
            }
        }
    }
}
