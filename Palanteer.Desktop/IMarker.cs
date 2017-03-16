using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    public interface IMarker : INotifyPropertyChanged
    {
        int X { get; set; }
        int Y { get; set; }
        string Name { get; set; }
    }
}
