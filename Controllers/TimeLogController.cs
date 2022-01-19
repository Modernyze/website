using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models.TimeLog;
using ModernyzeWebsite.Models.User;

namespace ModernyzeWebsite.Controllers;

public class TimeLogController : Controller {
    private readonly ModernyzeWebsiteContext db;

    public TimeLogController(ModernyzeWebsiteContext db) {
        this.db = db;
    }

    // GET: TimeLog
    public async Task<IActionResult> Index() {
        return View(await this.db.TimeLog.ToListAsync());
    }

    // POST: Punch In
    [HttpPost]
    public IActionResult PunchIn() {
        int userID = int.Parse(this.HttpContext.Session.GetString("UserId"));
        UserAccount user = GetUserAccountByID(userID);
        this.db.TimeLog.Add(new TimeLog {
            UserId = userID,
            PunchInTime = DateTime.Now,
            PunchOutTime = null
        });
        this.db.SaveChanges();
        return RedirectToAction("Index");
    }

    // POST: Punch Out
    [HttpPost]
    public IActionResult PunchOut() {
        int userID = int.Parse(this.HttpContext.Session.GetString("UserId"));
        UserAccount user = GetUserAccountByID(userID);
        TimeLog mostRecent = this.db.TimeLog.OrderByDescending(t => t.PunchInTime).Where(t => t.UserId == user.Id)
                                 .FirstOrDefault();
        mostRecent.PunchOutTime = DateTime.Now;
        this.db.SaveChanges();
        return RedirectToAction("Index");
    }

    private UserAccount GetUserAccountByID(int userID) {
        return this.db.UserAccount.Where(u => u.Id == userID).FirstOrDefault();
    }
}
