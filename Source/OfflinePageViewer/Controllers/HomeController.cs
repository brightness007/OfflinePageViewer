using Microsoft.AspNetCore.Mvc;

namespace OfflinePageViewer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
