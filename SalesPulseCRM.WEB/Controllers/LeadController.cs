using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesPulseCRM.Application.DTOs;
using SalesPulseCRM.Domain.Entities;
using SalesPulseCRM.Infrastructure.DB;
using Xunit.Sdk;

namespace SalesPulseCRM.WEB.Controllers
{
    public class LeadController : Controller
    {
        private readonly CrmDbContext _db;
        public LeadController(CrmDbContext crmDbContext)
        {
            _db = crmDbContext;
        }

        [HttpGet]
        public IActionResult CreateLead()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateLead(CreateLeadDto createLeadDto)
        {
            if(!ModelState.IsValid)
            {
                return View(createLeadDto);
            }

            //if(createLeadDto == null)
            //{

            //}

            var existingLead = await _db.Leads.FirstOrDefaultAsync(temp => temp.Phone == createLeadDto.Phone);

            if (existingLead != null)
            {
                ModelState.AddModelError("Phone", "Phone Number Already Exist");

                return View(createLeadDto);
            }

            var leads = new Lead { CustomerName = createLeadDto.CustomerName, Email = createLeadDto.Email, Phone = createLeadDto.Phone, LeadStatus = createLeadDto.LeadStatus };
                
            _db.Leads.Add(leads);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");

            
        }

    }
}
