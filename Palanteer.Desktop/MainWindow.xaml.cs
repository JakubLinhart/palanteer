using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Ultima;

namespace Palanteer.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer playerPositionRefreshTimer = new DispatcherTimer();
        private readonly Place player = new Place(canEdit: false) {Name = "Player"};

        public MainWindow()
        {
            InitializeComponent();

            _mapControl.PlacesCollection.Add(player);
            Task.Run(() => InitializePlayerPositionTracking());

            _placeControl.DataContext = new EditPlaceViewModel(player, _mapControl.PlacesCollection);
        }

        private void InitializePlayerPositionTracking()
        {
            Client.Calibrate();

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
            if (Client.FindLocation(ref x, ref y, ref z, ref facet))
            {
                player.X = x;
                player.Y = y;
            }
        }
    }
}
