using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class UnassignedLeadDto
    {
        public int TotalUnassigned { get; set; }
        public int Today { get; set; }
        public int ThreePlusDays { get; set; }
    }
}
