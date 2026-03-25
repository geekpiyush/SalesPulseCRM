using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
