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
    public ActionResult Register() {
        return View();
    }

    // POST: Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Register(UserAccount user) {
        return View();
    }

    // GET: Login
    public ActionResult Login() {
        return View();
    }

    // POST: Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Login(string email, string password) {
        return View();
    }

    // GET: Logout
    public ActionResult Logout() {
        this.HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
