using SalesPulseCRM.Domain.Enum;
using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public class Followup
{
    public int FollowupId { get; set; }

    public int LeadId { get; set; }
    public int UserId { get; set; }

    public DateTime FollowupDateTime { get; set; }
    public string? Remarks { get; set; }

    public FollowupStatus Status { get; set; } = FollowupStatus.Pending;
    public bool ReminderSent { get; set; } = false;

    // 🔗 Navigation
    public Lead Lead { get; set; } = null!;
}