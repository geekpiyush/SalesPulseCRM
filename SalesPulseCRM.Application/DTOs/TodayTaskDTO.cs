using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class TodayTaskDto
    {
        public int Followups { get; set; }
        public int Missed { get; set; }
        public int Meetings { get; set; }
    }
}
