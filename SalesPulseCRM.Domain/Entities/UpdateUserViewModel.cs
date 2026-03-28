using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Domain.Entities
{
    public class UpdateUserViewModel
    {
        public int UserId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string Role { get; set; }
        public int? ManagerId { get; set; }

        public bool IsActive { get; set; }

        public List<User> Managers { get; set; } = new();
    }
}
