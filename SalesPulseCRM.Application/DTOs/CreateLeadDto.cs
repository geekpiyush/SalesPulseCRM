using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{

    public class CreateLeadDto
    {
        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = null!;

        [Required]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public int? StateId { get; set; }
        [Required]
        public int? CityId { get; set; }
        [Required]
        public int? ProjectId { get; set; }
        public int? LeadSourceId { get; set; }
    }

}
