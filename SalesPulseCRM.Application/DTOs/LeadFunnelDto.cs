using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class LeadFunnelDto
    {
        public int TotalActive { get; set; }

        public int AttemptedContact { get; set; }
        public int NeedsFollowUp { get; set; }
        public int CallbackScheduled { get; set; }
        public int OnHold { get; set; }
        public int Interested { get; set; }

        public int Contacted { get; set; }
        public int Qualified { get; set; }
        public int Converted { get; set; }

        public int NoResponse { get; set; }
        public int NotInterested { get; set; }
        public int Lost { get; set; }
    }
}
