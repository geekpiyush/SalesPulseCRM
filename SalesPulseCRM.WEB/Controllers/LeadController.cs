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

            var userId = int.Parse(User.FindFirst("UserId").Value);
            var role = User.FindFirst(ClaimTypes.Role).Value;
            var allLeads = await _leadService.GetAllLeadsAsync(userId,role);

            return View(allLeads);
        }

        [HttpGet]
        public async Task<IActionResult> LeadsToAssign()
        {
            await LoadDropdowns();

            var userId = int.Parse(User.FindFirst("UserId").Value);
            var role = User.FindFirst(ClaimTypes.Role).Value;

            IQueryable<Lead> query = _db.Leads
                .Where(x => !x.IsDeleted);

            if (role == "Admin")
            {
                //Admin sees ALL unassigned leads
                query = query.Where(x => x.CurrentAssignedTo == null);
            }
            else if (role == "Manager")
            {
                var teamIds = await _db.Users
                    .Where(u => u.ManagerId == userId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                teamIds.Add(userId); // include manager himself

                query = query.Where(x =>
                    x.CurrentAssignedTo.HasValue &&
                    teamIds.Contains(x.CurrentAssignedTo.Value)
                );
            }
            else
            {
                // Employees should NOT access this page
                return Unauthorized();
            }

            var freshLeads = await (from l in query

                                    join u in _db.Users
                                    on l.CurrentAssignedTo equals u.UserId into ug
                                    from u in ug.DefaultIfEmpty()

                                    select new LeadAssignViewModel
                                    {
                                        LeadId = l.LeadId,
                                        CustomerName = l.CustomerName,
                                        Phone = l.Phone,
                                        ProjectName = l.Project != null ? l.Project.ProjectName : null,
                                        CityName = l.City != null ? l.City.CityName : null,
                                        Status = l.LeadStatus != null ? l.LeadStatus.StatusName : null,
                                        AssignedToName = u != null ? u.Name : "Unassigned",
                                        CreatedDate = l.CreatedDate
                                    })
                         .ToListAsync();

            // Employees list for assignment
            if (role == "Admin")
            {
                // Admin can assign to ALL employees
                ViewBag.Employees = await _db.Users
                    .Where(x => x.Role == "Manager" ||  x.Role == "Employee")
                    .ToListAsync();
            }
            else if (role == "Manager")
            {
                // Manager can assign ONLY to his team
                ViewBag.Employees = await _db.Users
                    .Where(x => x.Role == "Employee" && x.ManagerId == userId)
                    .ToListAsync();
            }

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

        public async Task<IActionResult> LeadUpdate(int leadId)
        {
            var data = await _leadService.GetLeadByIdAsync(leadId);

            await LoadDropdowns();
            return View (data);
        }


        [HttpPost]
        public async Task<IActionResult> LeadUpdate(LeadEditViewModel model)
        {
            try
            {
                // 🔥 FIX 1: Get userId correctly
                var userIdClaim = User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    await LoadDropdowns();

                    // 🔥 reload full data
                    var data = await _leadService.GetLeadByIdAsync(model.Lead.LeadId);

                    TempData["Error"] = "User not authenticated.";
                    return View("LeadUpdate", data);
                }

                int userId = int.Parse(userIdClaim);

                if (!ModelState.IsValid)
                {
                    await LoadDropdowns();

                    // 🔥 reload full data
                    var data = await _leadService.GetLeadByIdAsync(model.Lead.LeadId);
                     
                    TempData["Error"] = "Please fix validation errors.";
                    return View("LeadUpdate", data);
                }
                 
                var result = await _leadService.UpdateLeadAsync(model, userId);

                if (!result)
                {
                    await LoadDropdowns();

                    var data = await _leadService.GetLeadByIdAsync(model.Lead.LeadId);

                    TempData["Error"] = "Lead not found or update failed.";
                    return View("LeadUpdate", data);
                }

                TempData["Success"] = "Lead updated successfully ✅";

                return RedirectToAction("LeadUpdate", new { leadId = model.Lead.LeadId });
            }
            catch (Exception)
            {
                await LoadDropdowns();

                var data = await _leadService.GetLeadByIdAsync(model.Lead.LeadId);

                TempData["Error"] = "Something went wrong. Try again.";
                return View("LeadUpdate", data);
            }
        }
        public async Task<IActionResult> GetTimelinePartial(int leadId)
        {
            var timeline = await _leadService.GetTimeline(leadId);

            return PartialView("_LeadTimeLine", timeline);
        }

        public async Task<IActionResult> GetFilterLeads()
        {
            await  LoadDropdowns();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetFilteredLeadsData([FromBody] LeadFilterDto filter)
        {
            var query = _db.Leads
                .AsNoTracking()
                .Include(x => x.Project)
                .Include(x => x.LeadStatus)
                .AsQueryable();

            // Filters
            if (filter.LeadStatusId.HasValue)
                query = query.Where(x => x.LeadStatusId == filter.LeadStatusId);

            if (filter.ProjectId.HasValue)
                query = query.Where(x => x.ProjectId == filter.ProjectId);

            if (filter.UserId.HasValue)
                query = query.Where(x => x.CurrentAssignedTo == filter.UserId);

            if (filter.FromDate.HasValue)
                query = query.Where(x => x.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.CreatedDate <= filter.ToDate.Value);

            var data = await query
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    x.LeadId,
                    x.CustomerName,
                    Project = x.Project != null ? x.Project.ProjectName : "-",
                    Status = x.LeadStatus != null ? x.LeadStatus.StatusName : "-",

                    // 🔥 FIXED USER LOGIC
                    User = _db.Users
                        .Where(u => u.UserId == x.CurrentAssignedTo)
                        .Select(u => u.Name)
                        .FirstOrDefault(),

                    Date = x.CreatedDate.ToString("dd MMM yyyy")
                })
                .ToListAsync();

            return Json(data);
        }


       


    }
}
