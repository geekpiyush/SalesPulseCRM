using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.ServiceContracts;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

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
          _db=crmDbContext;
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


        [HttpGet("download-sample-file")]
        public async Task<IActionResult> DownloadSampleFile()
        {
            //var csv = new StringBuilder();
            //csv.AppendLine("CustomerName,Phone,Email,State,City,Project,Source");
            //csv.AppendLine("Piyush,9650261365,ddpiyush28@gmail.com,Delhi,Delhi,Project B,Meta");

            //var bytes = Encoding.UTF8.GetBytes(csv.ToString());

            //return File(bytes, "text/csv", "Lead_Import_Template.csv");

            var stream = await _leadService.GetExcelImportFile();

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Lead_Import_Template.xlsx"
            );
        }
        [HttpPost("import-leads")]
        public async Task<IActionResult> ImportLeads(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            // ✅ File type validation
            var allowedExtensions = new[] { ".xlsx" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only .xlsx files are allowed");

            try
            {
                var result = await _leadService.ImportLeadsFromExcel(file);

                return Ok(new
                {
                    successCount = result.successCount,
                    errorCount = result.errors.Count,
                    errors = result.errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
