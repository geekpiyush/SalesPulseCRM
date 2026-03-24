using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class UpdateLeadDto
    {
        public int LeadId { get; set; }

        public string CustomerName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }

        public int? LeadSourceId { get; set; }
        public int? LeadStatusId { get; set; }
    }
}
