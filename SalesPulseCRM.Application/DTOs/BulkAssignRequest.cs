using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class BulkAssignRequest
    {
        public List<int> LeadIds { get; set; } = new();
        public int UserId { get; set; }
    }
}
