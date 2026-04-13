using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesPulseCRM.Infrastructure.DB;

namespace SalesPulseCRM.WEB.Controllers
{
    [Authorize]
    public class FollowUpController : Controller
    {
        private readonly CrmDbContext _db;
        public FollowUpController(CrmDbContext crmDbContext)
        {
            _db = crmDbContext;
        }
        public IActionResult TodayFollowUp()
        {
            return View();
        }
    }
}
