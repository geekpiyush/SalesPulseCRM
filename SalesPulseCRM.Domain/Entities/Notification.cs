using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;

    public bool IsRead { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}