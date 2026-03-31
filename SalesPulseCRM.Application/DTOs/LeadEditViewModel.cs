using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class LeadEditViewModel
    {
        public UpdateLeadDto Lead { get; set; } = new();
        public List<TimelineItemDto> Timeline { get; set; } = new();
        // display fields
        public string CustomerName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? Email { get; set; }

        public string? ProjectName { get; set; }
        public string? CityName { get; set; }
        public string? CurrentAssignToName { get; set; }

        public string? LeadType { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
