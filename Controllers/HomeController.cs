using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModernyzeWebsite.Models;

namespace ModernyzeWebsite.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) {
        this._logger = logger;
    }

    public IActionResult Index() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier});
    }
}
