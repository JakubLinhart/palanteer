using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.Win32;
using Ultima;

namespace Palanteer.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer playerPositionRefreshTimer = new DispatcherTimer();
        private readonly IPlayerRepository playerRepository = new PlayerRepository(Client);
        private readonly ChatRepository chatRepository = new ChatRepository(Client);
        private readonly MapViewModel mapViewModel = new MapViewModel();
        private readonly ChatViewModel chatViewModel;

        protected override void OnClosing(CancelEventArgs e)
        {
            playerRepository.Delete(mapViewModel.Player.Id);
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = mapViewModel;

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _mapControl.Initialize(mapViewModel, new MapSource());
            mapViewModel.Player = new PlayerMarker(new Player() { Id = IdGenerator.Generate(), Name = "Player" });

            InitializeEditPlaceViewModel();
            Ultima.Client.Calibrate();

            chatViewModel = new ChatViewModel(mapViewModel.Player, chatRepository, this.Dispatcher);
            _chatControl.DataContext = chatViewModel;
        }

        private static readonly HttpClient Client = new HttpClient();

        public async Task ConnectToServer()
        {
            if (string.IsNullOrEmpty(mapViewModel.PalanteerUrl))
                return;

            Client.BaseAddress = new Uri(mapViewModel.PalanteerUrl);

            var places = await placeRepository.Get();
            Dispatcher.Invoke(() =>
            {
                foreach (var place in places)
                {
                    editPlaceViewModel.AddPlace(place);
                }
            });

            var lines = await this.chatRepository.GetHistory();
            Dispatcher.Invoke(() =>
            {
                chatViewModel.Lines = new ObservableCollection<ChatLine>(lines);
            });

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

            var hubConnection = new HubConnection(mapViewModel.PalanteerUrl);
            IHubProxy palanteerHubProxy = hubConnection.CreateHubProxy("PalanteerHub");
            palanteerHubProxy.On<Player>("PlayerUpdated", OnRemotePlayerUpdated);
            palanteerHubProxy.On<ChatLine>("ChatLineAdded", this.chatRepository.OnChatLineAdded);
            hubConnection.Start().Wait();

            playerPositionRefreshTimer.Tick += PlayerPositionRefreshTimerOnTick;
            playerPositionRefreshTimer.Interval = TimeSpan.FromSeconds(1);
            playerPositionRefreshTimer.Start();
        }

        private void InitializeEditPlaceViewModel()
        {
            placeRepository = new HttpRestPlaceRepository(Client);
            editPlaceViewModel = new EditPlaceViewModel(mapViewModel.Player, mapViewModel.Places, placeRepository);
            _placeControl.DataContext = editPlaceViewModel;
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
        private EditPlaceViewModel editPlaceViewModel;
        private HttpRestPlaceRepository placeRepository;

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

        private void XRayOnChecked(object sender, RoutedEventArgs e)
        {
            _mapControl.XRay = true;
        }

        private void XRayOnUnchecked(object sender, RoutedEventArgs e)
        {   
            _mapControl.XRay = false;
        }

        private void GoButtonClicked(object sender, RoutedEventArgs e)
        {
            if (PositionParser.TryParse(_positionTextBox.Text, out Point position))
            {
                mapViewModel.TrackedPlayer = null;
                mapViewModel.SelectedPosition = position;
            }
        }

        private void OnImportButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "UOAutomap files (*.map)|*.map";
            if (openFileDialog.ShowDialog() == true)
            {
                string importedText = File.ReadAllText(openFileDialog.FileName);
                var importedPlaces = UoAutomapImporter.Import(importedText);
                var importWindow = new AutomapImportWindow(importedPlaces, this.editPlaceViewModel);
                importWindow.ShowDialog();
            }
        }

        private void OnConnectButtonClicked(object sender, RoutedEventArgs e)
        {
            ConnectToServer();
        }
    }
}
