using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
    public class MvcController : Controller {
        [HttpGet("/mvc/verify")]
        public IActionResult Index() {
            return View("/View/Mvc/Index.cshtml");
        }
    }
}
