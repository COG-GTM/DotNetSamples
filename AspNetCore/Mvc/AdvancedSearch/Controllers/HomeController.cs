using Microsoft.AspNetCore.Mvc;

namespace EqDemo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
