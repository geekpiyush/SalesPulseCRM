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

        //[HttpGet("today-tasks")]
        //public async Task<IActionResult> GetTodayTasks()
        //{
        //    var userIdClaim = User.FindFirst("UserId")?.Value;
        //    var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        //    if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
        //    {
        //        return Unauthorized("Invalid user context");
        //    }

        //    if (!int.TryParse(userIdClaim, out int userId))
        //    {
        //        return BadRequest("Invalid UserId");
        //    }

        //    var result = await _leadService.GetTodayTasksAsync(userId, roleClaim);

        //    return Ok(result);
        //}

        [HttpGet("leads-count")]
        public async Task<IActionResult> GetLeadsCount()
        {
            var userIdClaim = int.Parse(User.FindFirst("UserId")?.Value);
            var roleClaim = User.FindFirst(ClaimTypes.Role).Value;

            var data = await _leadService.GetTotalLeadCount(userIdClaim, roleClaim);
            return Ok(data);
        }

        [HttpGet("unassigned-leads")]
        public async Task<IActionResult> GetAllUnassignedLeads()
        {
            var data = await _leadService.GetTotalUnassignedLead();
            return Ok(data);
        }

        [HttpGet("converted-leads")]
        public async Task<IActionResult> GetAllConvertedLeadsCount()
        {
            var userId =int.Parse(User.FindFirst("UserId").Value);
           var roleClaim = User.FindFirst(ClaimTypes.Role).Value;

            var data = await _leadService.GetConvertedLeads(userId, roleClaim);
            return Ok(data);
        }

        [HttpGet("lead-funnel")]
        public async Task<IActionResult> GetLeadFunnel()
        {
            var userId = int.Parse(User.FindFirst("UserId").Value);
            var roleClaim = User.FindFirst(ClaimTypes.Role).Value;

            var data = await _leadService.GetLeadFunnelAsync(userId, roleClaim);
            return Ok(data);
        }
    }
}
