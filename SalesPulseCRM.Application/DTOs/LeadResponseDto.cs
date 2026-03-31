using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class LeadResponseDto
    {
        
            public int LeadId { get; set; }

            public string CustomerName { get; set; } = null!;

            public string Phone { get; set; } = null!;

            public string? Email { get; set; }

            public string? StateName { get; set; }
            public string? CityName { get; set; }
            public int? ProjectId { get; set; }
            public string? ProjectName { get; set; }
            public string? SourceName { get; set; }
            public string? LeadType { get; set; }
            public int? CurrentAssignTo { get; set; }
            public string? CurrentAssignToName { get; set; }
        

            public DateTime CreatedDate { get; set; }
        
    }
}
