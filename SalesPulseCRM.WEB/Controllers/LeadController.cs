using Azure.Core;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.ServiceContracts;
using SalesPulseCRM.Application.Services;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;
using System.Security.Claims;
using Xunit.Sdk;

namespace SalesPulseCRM.WEB.Controllers
{
    [Authorize]
    public class LeadController : Controller
    {
        private readonly CrmDbContext _db;
        private readonly ILeadService _leadService;
        private readonly EmailServices _emailServices;

        private async Task LoadDropdowns()
        {
            ViewBag.Sources = await _db.LeadSources.ToListAsync();
            ViewBag.Users = await _db.Users.ToListAsync();
            ViewBag.States = await _db.States.ToListAsync();
            ViewBag.LeadStatus = await _db.LeadStatus.ToListAsync();
            ViewBag.Projects = await _db.Projects
                .Where(p => p.IsActive)
                .ToListAsync();
        }
        public LeadController(CrmDbContext crmDbContext, ILeadService leadService, EmailServices emailServices)
        {
            _db = crmDbContext;
            _leadService = leadService;
            _emailServices = emailServices;
        }

        [HttpGet("GetCities")]
        public async Task<IActionResult> GetCities(int stateId)
        {
            var cities = await _db.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new
                {
                    c.CityId,
                    c.CityName
                })
                .ToListAsync();

            return Json(cities);
        }

        [HttpGet]
        public async Task<IActionResult> CreateLead()
        {
            await LoadDropdowns();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateLead(CreateLeadDto createLeadDto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(createLeadDto);
            }

            var result = await _leadService.CreateLeadAsync(createLeadDto);
            if (!result.success)
            {
                ViewBag.Error = result.message;
                await LoadDropdowns();
                return View(createLeadDto);
            }

            TempData["Success"] = result.message;
            return RedirectToAction("CreateLead", "Lead");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLeads()
        {
            await LoadDropdowns();
            var allLeads = await _leadService.GetAllLeadsAsync();

            return View(allLeads);
        }

        [HttpGet]
        public async Task<IActionResult> LeadsToAssign()
        {
            await LoadDropdowns();
            var freshLeads = await _db.Leads
       .Where(x => x.CurrentAssignedTo == null && !x.IsDeleted)
       .Include(x => x.City)
       .Include(x => x.Project)
       .Include(x => x.LeadStatus)
       .ToListAsync();

            ViewBag.Employees = await _db.Users.Where(x => x.Role == "Employee").ToListAsync();

            return View(freshLeads);
        }


        [HttpPost]
        public async Task<IActionResult> AssignLead([FromBody]AssignLeadRequest assignLeadRequest)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId").Value);

            var lead = await _db.Leads.FindAsync(assignLeadRequest.LeadId);
            if(lead == null)
            {
                return NotFound("Lead not found");
            }

            if (!User.IsInRole("Manager") && !User.IsInRole("Admin"))
            {
                return Unauthorized();
            }

            _db.LeadAssignments.Add(new LeadAssignment
            {
                LeadId = lead.LeadId,
                AssignedTo = assignLeadRequest.UserId,
                AssignedBy = currentUserId,
                AssignedDate = DateTime.UtcNow

            });

            lead.CurrentAssignedTo = assignLeadRequest.UserId;
            lead.CurrentAssignedBy = currentUserId;
            lead.CurrentAssignedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // ✅ SAFE DTO FETCH
            var leadDto = await _db.Leads
                .Where(x => x.LeadId == assignLeadRequest.LeadId)
                .Select(x => new LeadEmailDto
                {
                    CustomerName = x.CustomerName,
                    Phone = x.Phone,
                    Email = x.Email,
                    ProjectName = x.Project != null ? x.Project.ProjectName : "",
                    CityName = x.City != null ? x.City.CityName : ""
                })
                .FirstOrDefaultAsync();


            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == assignLeadRequest.UserId);

            if (user != null)
            {
                BackgroundJob.Enqueue(() =>

                   _emailServices.SendLeadAssignEmail(user.Email,
                user.Name,
                leadDto.CustomerName,
                leadDto.Phone,
                leadDto.Email,
                leadDto.ProjectName,
                leadDto.CityName)

                 );
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> BulkAssignLead([FromBody] BulkAssignRequest request)
        {
            var currentUserId = int.Parse(User.FindFirst("UserId").Value);

            var leads = await _db.Leads
                .Where(x => request.LeadIds.Contains(x.LeadId))
                .ToListAsync();

            if (!leads.Any())
                return NotFound("No leads found");

            if (!User.IsInRole("Manager") && !User.IsInRole("Admin"))
            {
                return Unauthorized();
            }

            foreach (var lead in leads)
            {
                // 🔥 HISTORY
                _db.LeadAssignments.Add(new LeadAssignment
                {
                    LeadId = lead.LeadId,
                    AssignedTo = request.UserId,
                    AssignedBy = currentUserId,
                    AssignedDate = DateTime.UtcNow
                });

                // 🔥 CURRENT
                lead.CurrentAssignedTo = request.UserId;
                lead.CurrentAssignedBy = currentUserId;
                lead.CurrentAssignedDate = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            var leadDtos = await _db.Leads
            .Where(x => request.LeadIds.Contains(x.LeadId))
            .Select(x => new LeadEmailDto
            {
                CustomerName = x.CustomerName,
                Phone = x.Phone,
                Email = x.Email,
                ProjectName = x.Project != null ? x.Project.ProjectName : "",
                CityName = x.City != null ? x.City.CityName : ""
            })
            .ToListAsync();


            foreach (var lead in leads)
                {
                    var user = await _db.Users.FirstOrDefaultAsync(x => x.UserId == request.UserId);

                if (user != null && leadDtos.Any())
                {
                    BackgroundJob.Enqueue(() =>
                        _emailServices.SendBulkLeadAssign(
                            user.Email,
                            user.Name,
                            leadDtos
                        )
                    );
                }
            }
          return Ok();
        }
    }
}
