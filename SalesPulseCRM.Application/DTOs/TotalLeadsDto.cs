using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class TotalLeadsDto
    {
        public int TotalActiveLeads { get; set; }
        public int LostLeads { get; set; }

    }
}
