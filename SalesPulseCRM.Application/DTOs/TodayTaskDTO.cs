using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class UserTaskDto

    {
        public int UserId { get; set; }

        public string Name { get; set; }
        public string Role { get; set; }

        public int Followups { get; set; }
        public int Missed { get; set; }
        public int Meetings { get; set; }
        public int TotalLeads { get; internal set; }
    }
}
