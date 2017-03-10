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

namespace Palanteer.Desktop
{
    public partial class EditPlaceControl : UserControl
    {
        public EditPlaceControl()
        {
            InitializeComponent();
        }

        private async void OnNewButtonClick(object sender, RoutedEventArgs e)
        {
            await ((EditPlaceViewModel) DataContext).NewPlace();
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            ((EditPlaceViewModel)DataContext).DeletePlace();
        }
    }
}
