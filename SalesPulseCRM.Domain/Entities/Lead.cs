using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public partial class Lead
{
    public int LeadId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public int? LeadSourceId { get; set; }

    public string LeadStatus { get; set; } = null!;

    public int NotesCount { get; set; }

    public int? AssignedTo { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }
}
