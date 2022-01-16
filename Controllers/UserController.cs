using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models;

namespace ModernyzeWebsite.Controllers; 

public class UserController : Controller {
    private readonly ModernyzeWebsiteContext db;

    public UserController(ModernyzeWebsiteContext context) {
        this.db = context;
    }

    // GET: User
    public async Task<IActionResult> Index() {
        return View(await this.db.UserAccount.ToListAsync());
    }
    
    // GET: Register
    public Task<IActionResult> Register() {
        return View();
    }

    // POST: Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Register(UserAccount user) {

        return View();
    }

    public Task<IActionResult> Login() {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Login(string email, string password) {
        return View();
    }

    public ActionResult Logout() {
        this.HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
