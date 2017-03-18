using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ultima;

namespace Palanteer.Desktop
{
    /// <summary>
    /// Interaction logic for SelectUltimaClient.xaml
    /// </summary>
    public partial class SelectUltimaClient : UserControl
    {
        public SelectUltimaClient()
        {
            InitializeComponent();

            LoadClients();
        }

        private void LoadClients()
        {
            var clients = Client.FindWindowsWithText("Ultima Online")
                .Select(h => new UltimaClient() {Handle = h, Title = Client.GetWindowText(h)})
                .ToArray();

            _clientsComboBox.ItemsSource = clients;
            if (clients.Any())
                _clientsComboBox.SelectedItem = clients.First();
        }

        private void _clientsComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IntPtr selectedValue = (IntPtr)_clientsComboBox.SelectedValue;

            Task.Run(() =>
            {
                Client.Handle = new ClientWindowHandle(selectedValue);
                Client.Calibrate();
            });
        }

        private void RefreshOnClick(object sender, RoutedEventArgs e)
        {
            LoadClients();
        }
    }
}
