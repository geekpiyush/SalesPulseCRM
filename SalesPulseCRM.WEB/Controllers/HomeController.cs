using Microsoft.AspNetCore.Mvc;

namespace SalesPulseCRM.WEB.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
