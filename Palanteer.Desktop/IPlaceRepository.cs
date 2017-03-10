using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Palanteer.Desktop
{
    public interface IPlaceRepository
    {
        Task Create(Place place);
        Task Update(Place place);
        Task Delete(Place place);
        Task<IEnumerable<Place>> Get();
    }
}
