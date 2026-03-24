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
           ViewBag.Sources = _db.LeadSources.ToList();
            ViewBag.Users = _db.Users.ToList();
            return View();
        }

 

    }
}
