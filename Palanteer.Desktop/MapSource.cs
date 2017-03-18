using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Ultima;

namespace Palanteer.Desktop
{
    public class MapSource
    {
        private Task<BitmapImage> GetMapImage(string cacheFileName, bool xray)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(cacheFileName))
                {
                    var bitmap = Map.Felucca.GetImage(0, 0, 768, 512, xray);
                    using (FileStream stream = File.Create(cacheFileName))
                    {
                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                if (File.Exists(cacheFileName))
                {
                    BitmapImage image = null;
                    Application.Current.Dispatcher.Invoke(
                        () => image = new BitmapImage(new Uri("pack://siteoforigin:,,,/" + cacheFileName)));
                    return image;
                }

                return null;
            });
        }

        public Task<BitmapImage> GetMapImage()
        {
            return GetMapImage("map.png", true);
        }

        public Task<BitmapImage> GetXRayMapImage()
        {
            return GetMapImage("map-xray.png", false);
        }
    }
}
