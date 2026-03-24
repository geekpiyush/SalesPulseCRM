using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Entities
{
    public class LeadStatus
    {
        public int LeadStatusId { get; set; }
        public string StatusName { get; set; } = null!;

        public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    }
}
