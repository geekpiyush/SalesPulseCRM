using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Application.ServiceContracts;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;
using Xunit.Sdk;

namespace SalesPulseCRM.WEB.Controllers
{
    [Route("Lead")]
    public class LeadController : Controller
    {
        private readonly CrmDbContext _db;
        private readonly ILeadService _leadService;

        private async Task LoadDropdowns()
        {
            ViewBag.Sources = await _db.LeadSources.ToListAsync();
            ViewBag.Users = await _db.Users.ToListAsync();
            ViewBag.States = await _db.States.ToListAsync();
            ViewBag.Projects = await _db.Projects
                .Where(p => p.IsActive)
                .ToListAsync();
        }
        public LeadController(CrmDbContext crmDbContext, ILeadService leadService)
        {
            _db = crmDbContext;
            _leadService = leadService;
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
            if(!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(createLeadDto);
            }

            var result = await _leadService.CreateLeadAsync(createLeadDto);
            if(!result.success)
            {
                ViewBag.Error = result.message;
                await LoadDropdowns();
                return View(createLeadDto);
            }

            TempData["Success"] = result.message;
            return RedirectToAction("CreateLead", "Lead");
        }

    }
}
