using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Infrastructure.DB;

public partial class LeadSource
{
    public int LeadSourceId { get; set; }

    public string SourceName { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}
