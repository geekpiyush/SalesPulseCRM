using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.DTOs
{
    public class TimelineItemDto
    {
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public string? UserName { get; set; }
    }
}
