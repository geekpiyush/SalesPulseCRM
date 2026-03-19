using Microsoft.AspNetCore.Mvc;

namespace SalesPulseCRM.WEB.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }
    }
}
