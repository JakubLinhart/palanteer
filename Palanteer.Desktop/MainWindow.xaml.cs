using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR.Client;
using Ultima;

namespace Palanteer.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer playerPositionRefreshTimer = new DispatcherTimer();
        private readonly Marker player = new Marker(null, canEdit: false) {Name = "Player"};
        private readonly IPlayerRepository playerRepository = new PlayerRepository(Client );
        private readonly string playerId = IdGenerator.Generate();

        protected override void OnClosing(CancelEventArgs e)
        {
            playerRepository.Delete(playerId);
        }

        public MainWindow()
        {
            InitializeComponent();
            
            _mapControl.MarkersCollection.Add(player);

            Client.BaseAddress = new Uri("http://localhost:2044/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            InitializeEditPlaceViewModel();

            Task.Run(() => InitializePlayerPositionTracking());
            Task.Run(() => LoadRemotePlayers());
        }

        private async Task LoadRemotePlayers()
        {
            var allPlayers = await playerRepository.GetAll();
            var markers = allPlayers.Where(p => p.Id != playerId)
                .Select(p => new PlayerMarker(p));

            this.Dispatcher.Invoke(() =>
            {
                foreach (var marker in markers)
                {
                    this.remotePlayerMarkers[marker.PlayerId] = marker;
                    _mapControl.MarkersCollection.Add(marker);
                }
            });
        }

        private static readonly HttpClient Client = new HttpClient();

        private void InitializeEditPlaceViewModel()
        {
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

            var hubConnection = new HubConnection("http://localhost:2044/");
            IHubProxy palanteerHubProxy = hubConnection.CreateHubProxy("PalanteerHub");
            palanteerHubProxy.On<Player>("PlayerUpdated", OnRemotePlayerUpdated);
            hubConnection.Start().Wait();

            playerPositionRefreshTimer.Tick += PlayerPositionRefreshTimerOnTick;
            playerPositionRefreshTimer.Interval = TimeSpan.FromSeconds(1);
            playerPositionRefreshTimer.Start();
        }

        private void OnRemotePlayerUpdated(Player player)
        {
            Dispatcher.Invoke(() =>
            {
                if (!remotePlayerMarkers.TryGetValue(player.Id, out PlayerMarker marker))
                {
                    marker = new PlayerMarker(player);
                    remotePlayerMarkers[player.Id] = marker;

                    _mapControl.MarkersCollection.Add(marker);
                }
                else
                    marker.UpdateFromModel(player);
            });
        }

        private void PlayerPositionRefreshTimerOnTick(object sender, EventArgs eventArgs)
        {
            RefreshPlayerPosition();
        }

        Dictionary<string, PlayerMarker> remotePlayerMarkers = new Dictionary<string, PlayerMarker>();

        private void RefreshPlayerPosition()
        {
            int x = 0, y = 0, z = 0, facet = 0;
            if (Ultima.Client.FindLocation(ref x, ref y, ref z, ref facet))
            {
                if (player.X != x || player.Y != y)
                {
                    player.X = x;
                    player.Y = y;
                    playerRepository.Update(new Player() {Id = playerId, Name = "Player", X = x, Y = y});
                }
            }
        }
    }
}
