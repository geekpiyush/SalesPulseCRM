using Microsoft.AspNetCore.Mvc;

namespace SalesPulseCRM.WEB.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
