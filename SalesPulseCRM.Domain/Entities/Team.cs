using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Entities
{
    public class Team
    {
        public int TeamId { get; set; }

        public string TeamName { get; set; }

        public int ManagerId { get; set; } // 🔥 important

        public User Manager { get; set; }

        public ICollection<TeamMember> Members { get; set; }
    }
}
