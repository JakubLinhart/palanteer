using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Palanteer.Desktop
{
    public static class PositionParser
    {
        public static bool TryParse(string positionText, out Point point)
        {
            point = default(Point);

            var parts = positionText.Split(' ');
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out int x))
                return false;
            if (!int.TryParse(parts[1], out int y))
                return false;

            point = new Point(x, y);

            return true;
        }
    }
}
