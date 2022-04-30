using Microsoft.AspNetCore.Mvc;

namespace PotatoBot.Controllers
{
    public class TelegramController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
