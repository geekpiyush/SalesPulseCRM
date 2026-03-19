using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Infrastructure.DB;

public partial class Followup
{
    public int FollowupId { get; set; }

    public int LeadId { get; set; }

    public int UserId { get; set; }

    public DateTime FollowupDateTime { get; set; }

    public string? Remarks { get; set; }

    public string Status { get; set; } = null!;

    public bool ReminderSent { get; set; }
}
