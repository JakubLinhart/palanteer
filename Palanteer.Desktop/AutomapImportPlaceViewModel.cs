using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    public class AutomapImportPlaceViewModel : INotifyPropertyChanged
    {
        public Place Place { get; }
        private bool selected;

        public AutomapImportPlaceViewModel(Place place)
        {
            this.Place = place;
            selected = true;
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        public string Description => $"{Place.Name} ({Place.Type})";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
