using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Entities
{
    public class TeamViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string ManagerName { get; set; }
        public int MemberCount { get; set; }
        public List<string> MemberNames { get; set; } = new();
    }
}
