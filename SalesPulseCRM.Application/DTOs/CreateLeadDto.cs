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

        [Required(ErrorMessage ="Customer Name Can't be Blank")] 
            public string CustomerName { get; set; } = null!;

        [Required(ErrorMessage = "Phone Number Can't Blank")]
             public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Email Number Can't Blank")]
             public string? Email { get; set; }

             public string LeadSource { get; set; } = "Meta";

            public string LeadStatus { get; set; } = "New"; // default

            //public int? AssignedTo { get; set; }
        }
    
}
