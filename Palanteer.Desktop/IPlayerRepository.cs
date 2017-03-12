using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    public interface IPlayerRepository
    {
        Task<Player[]> GetAll();
        Task Update(Player player);
        Task Delete(string playerId);
    }
}
