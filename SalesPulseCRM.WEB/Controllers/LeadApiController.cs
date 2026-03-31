using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesPulseCRM.Application.ServiceContracts;

namespace SalesPulseCRM.WEB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadApiController : ControllerBase
    {
        private readonly ILeadService _leadService;
        public LeadApiController(ILeadService leadService)
        {
            _leadService=leadService;
        }

        [HttpGet("today-tasks")]
        public async Task<IActionResult> GetTodayTasks()
        {
            var result = await _leadService.GetTodayTasksAsync();
            return Ok(result);
        }
    }
}
