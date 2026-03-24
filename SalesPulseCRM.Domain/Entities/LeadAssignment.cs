using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public class LeadAssignment
{
    public int AssignmentId { get; set; }

    public int LeadId { get; set; }
    public int AssignedBy { get; set; }
    public int AssignedTo { get; set; }

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    // 🔗 Navigation
    public Lead Lead { get; set; } = null!;
}