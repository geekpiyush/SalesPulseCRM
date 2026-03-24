using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public class Lead
{
    public int LeadId { get; set; }

    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }

    public int? LeadSourceId { get; set; }
    public int? LeadStatusId { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedDate { get; set; }

    public bool IsDeleted { get; set; } = false;

    // 🔗 Navigation
    public LeadSource? LeadSource { get; set; }
    public LeadStatus? LeadStatus { get; set; }

    public ICollection<LeadAssignment> Assignments { get; set; } = new List<LeadAssignment>();
    public ICollection<LeadNote> Notes { get; set; } = new List<LeadNote>();
    public ICollection<Followup> Followups { get; set; } = new List<Followup>();
}
