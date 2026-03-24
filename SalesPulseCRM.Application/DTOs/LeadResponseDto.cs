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

        public string? LeadSource { get; set; }
        public string? LeadStatus { get; set; }

        public DateTime CreatedDate { get; set; }

        public int NotesCount { get; set; }
        public int FollowupCount { get; set; }

        public int? LastAssignedTo { get; set; }
    }
}
