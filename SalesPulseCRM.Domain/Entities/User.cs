using System;
using System.Collections.Generic;

namespace SalesPulseCRM.Domain.Entities;
public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? ManagerId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<User> InverseManager { get; set; } = new List<User>();

    public virtual User? Manager { get; set; }
}
