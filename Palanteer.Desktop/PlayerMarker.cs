using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    internal sealed class PlayerMarker : Marker
    {
        public string PlayerId { get; }

        public PlayerMarker(Player player) : base(player, false)
        {
            PlayerId = player.Id;

            UpdateFromModel(player);
        }

        public void UpdateFromModel(Player player)
        {
            X = player.X;
            Y = player.Y;
            Name = player.Name;
        }
    }
}
