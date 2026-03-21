using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string NotificationType { get; set; } = null!;

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; }
}
