using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public partial class LeadNote
{
    public int NoteId { get; set; }

    public int LeadId { get; set; }

    public int UserId { get; set; }

    public string NoteText { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}
