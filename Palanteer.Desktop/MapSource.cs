using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Ultima;

namespace Palanteer.Desktop
{
    public class MapSource
    {
        public BitmapImage GetMapImage()
        {
            if (!File.Exists("map.png"))
            {
                var bitmap = Map.Felucca.GetImage(0, 0, 768, 512, true);
                using (FileStream stream = File.Create("map.png"))
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                }
            }

            if (File.Exists("map.png"))
            {
                var image = new BitmapImage(new Uri("pack://siteoforigin:,,,/map.png"));
                return image;
            }

            return null;
        }
    }
}
