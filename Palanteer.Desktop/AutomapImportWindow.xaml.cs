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
using System.Windows.Shapes;

namespace Palanteer.Desktop
{
    public partial class AutomapImportWindow : Window
    {
        private readonly EditPlaceViewModel editPlaceViewModel;

        public AutomapImportWindow(Place[] importedPlaces, EditPlaceViewModel editPlaceViewModel)
        {
            this.editPlaceViewModel = editPlaceViewModel;
            InitializeComponent();

            _importListBox.ItemsSource = importedPlaces.Select(p => new AutomapImportPlaceViewModel(p)).ToArray();
        }

        private void OnImportButtonClicked(object sender, RoutedEventArgs e)
        {
            var viewModels = (IEnumerable<AutomapImportPlaceViewModel>) _importListBox.ItemsSource;
            foreach (var viewModel in viewModels)
            {
                if (viewModel.Selected)
                    editPlaceViewModel.NewPlace(viewModel.Place);
            }
        }
    }
}
