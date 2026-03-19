using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Infrastructure.DB;

public partial class EmailQueue
{
    public int EmailId { get; set; }

    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? SentDate { get; set; }
}
