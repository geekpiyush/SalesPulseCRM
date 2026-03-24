using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public class LeadSource
{
    public int LeadSourceId { get; set; }
    public string SourceName { get; set; } = null!;

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}