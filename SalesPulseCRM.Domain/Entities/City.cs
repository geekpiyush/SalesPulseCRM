using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Entities
{
    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = null!;

        public int StateId { get; set; }
        public State State { get; set; } = null!;
    }
}
