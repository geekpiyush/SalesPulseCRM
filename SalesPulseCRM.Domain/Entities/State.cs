using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Entities
{
    public class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; } = null!;

        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
