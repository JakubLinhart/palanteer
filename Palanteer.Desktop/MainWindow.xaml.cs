using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR.Client;
using Ultima;

namespace Palanteer.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer playerPositionRefreshTimer = new DispatcherTimer();
        private readonly IPlayerRepository playerRepository = new PlayerRepository(Client );
        private readonly MapViewModel mapViewModel = new MapViewModel();

        protected override void OnClosing(CancelEventArgs e)
        {
            playerRepository.Delete(mapViewModel.Player.Id);
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = mapViewModel;

            Client.BaseAddress = new Uri("http://localhost:2044/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _mapControl.Initialize(mapViewModel, new MapSource());
            mapViewModel.Player = new PlayerMarker(new Player() { Id = IdGenerator.Generate(), Name = "Player" });

            InitializeEditPlaceViewModel();
            Task.Run(() => InitializePlayerPositionTracking());
            Task.Run(() => LoadRemotePlayers());
        }

        private async Task LoadRemotePlayers()
        {
            var allPlayers = await playerRepository.GetAll();
            var markers = allPlayers.Where(p => p.Id != mapViewModel.Player.Id)
                .Select(p => new PlayerMarker(p));

            this.Dispatcher.Invoke(() =>
            {
                foreach (var marker in markers)
                {
                    this.remotePlayerMarkers[marker.Id] = marker;
                    mapViewModel.Players.Add(marker);
                }
            });
        }

        private static readonly HttpClient Client = new HttpClient();

        private void InitializeEditPlaceViewModel()
        {
            var placeRepository = new HttpRestPlaceRepository(Client);
            var placeViewModel = new EditPlaceViewModel(mapViewModel.Player, mapViewModel.Places, placeRepository);
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
            if (player.Id != mapViewModel.Player.Id)
            {
                Dispatcher.Invoke(() =>
                {
                    if (!remotePlayerMarkers.TryGetValue(player.Id, out PlayerMarker marker))
                    {
                        marker = new PlayerMarker(player);
                        remotePlayerMarkers[player.Id] = marker;

                        mapViewModel.Players.Add(marker);
                    }
                    else
                        marker.UpdateFromModel(player);
                });
            }
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
                if (mapViewModel.Player.X != x || mapViewModel.Player.Y != y)
                {
                    mapViewModel.Player.X = x;
                    mapViewModel.Player.Y = y;
                    playerRepository.Update(mapViewModel.Player.Model);
                }
            }
            else
            {
                Ultima.Client.Calibrate();
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (sender is ListBox listBox)
                {
                    var valid = e.AddedItems[0];
                    foreach (var item in new ArrayList(listBox.SelectedItems))
                    {
                        if (item != valid)
                        {
                            listBox.SelectedItems.Remove(item);
                        }
                    }
                }
            }
        }
    }
}
