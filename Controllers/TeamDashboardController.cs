using Microsoft.AspNetCore.Mvc;

namespace ModernyzeWebsite.Controllers;

public class TeamDashboardController : Controller {

    public IActionResult Index() {
        return View();
    }
}
