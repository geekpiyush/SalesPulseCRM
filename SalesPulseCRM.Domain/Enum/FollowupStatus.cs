using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Enum
{
    public enum FollowupStatus
    {
        Pending = 1,     // created but future
        Due = 2,         // time reached
        Completed = 3,   // done
        Missed = 4
    }
}
