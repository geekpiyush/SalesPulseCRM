using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class LeadAssignViewModel
    {
        public int LeadId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string? ProjectName { get; set; }
        public string? CityName { get; set; }
        public string? Status { get; set; }
        public string AssignedToName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
