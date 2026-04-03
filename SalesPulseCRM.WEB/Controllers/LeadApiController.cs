using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.ServiceContracts;
using SalesPulseCRM.Infrastructure.DB;
using System.Security.Claims;

namespace SalesPulseCRM.WEB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadApiController : ControllerBase
    {
        private readonly ILeadService _leadService;
        private readonly CrmDbContext _db;
        public LeadApiController(ILeadService leadService, CrmDbContext crmDbContext)
        {
            _leadService=leadService;
          
        }

        [HttpGet("today-tasks")]
        public async Task<IActionResult> GetTodayTasks()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
            {
                return Unauthorized("Invalid user context");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid UserId");
            }

            var result = await _leadService.GetTodayTasksAsync(userId, roleClaim);

            return Ok(result);
        }

        //[HttpGet("unassigned-leads")]
        //public async Task<IActionResult> GetUnassignedLeads()
        //{
        //    var result = await _leadService.GetUnassignedLeadsAsync();
        //    return Ok(result);
        //}

        //[HttpGet("total-leads")]
        //public async Task<IActionResult> GetTotalLeads()
        //{
        //    try
        //    {
        //        var userId = int.Parse(User.FindFirst("UserId")?.Value);
        //        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        //        var result = await _leadService.GetTotalLeadsCount();
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message); // 👈 temporarily show real error
        //    }
        //}

        //[HttpGet("converted-leads")]
        //public async Task<IActionResult> GetConvertedStats()
        //{
        //    var data = await _leadService.GetAllConvertedLeads();
        //    return Ok(data);
        //}
    }
}
