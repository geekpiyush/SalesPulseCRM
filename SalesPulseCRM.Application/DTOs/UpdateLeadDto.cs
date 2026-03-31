using SalesPulseCRM.Domain.Enum;
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

        public int? LeadStatusId { get; set; }

        public DateTime? FollowupDate { get; set; }

        public MeetingStatus? MeetingStatus { get; set; }
        public DateTime? MeetingDateTime { get; set; }

        public CustomerInterest? CustomerInterest { get; set; }
        public NextAction? NextAction { get; set; }
        public Budget? Budget { get; set; }

        public int? ProjectId { get; set; }

        public string? NoteText { get; set; }
    }
}
