using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Mvc;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models;
using NuGet.Frameworks;

namespace ModernyzeWebsite.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly ModernyzeWebsiteContext db;

    public HomeController(ILogger<HomeController> logger, ModernyzeWebsiteContext db) {
        this._logger = logger;
        this.db = db;
    }

    public IActionResult Index() {
        return View(DisplayTimeLoggedThisWeek());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier});
    }

    private List<TimeLogViewModel> DisplayTimeLoggedThisWeek() {
        DateTime startDate = StartOfWeek();
        DateTime endDate = StartOfWeek().AddDays(7);
        IEnumerable<TimeLog> list = this.db.TimeLog.Where(t => t.PunchInTime >= startDate && t.PunchInTime <= endDate).ToList();
        Dictionary<int, TimeLogViewModel> byUser = new();
        foreach (TimeLog log in list) {
            if (byUser.ContainsKey(log.UserId)) {
                TimeLogViewModel current = byUser[log.UserId];
                TimeSpan sum =
                    current.TimeLogged.Add(GetTimeDifference(log.PunchInTime, log.PunchOutTime.GetValueOrDefault()));
                current.TimeLogged = sum;
                continue;
            }

            byUser.Add(log.UserId, new TimeLogViewModel {
                FullName = GetUserByID(log.UserId).FullName,
                TimeLogged = GetTimeDifference(log.PunchInTime, log.PunchOutTime.GetValueOrDefault())
            });
        }

        return byUser.Values.ToList();
    }

    private static DateTime StartOfWeek() {
        int diff = (7 + (DateTime.Now.DayOfWeek - DayOfWeek.Sunday)) % 7;
        DateTime startOfWeek = DateTime.Now.AddDays(-1 * diff).Date;
        return startOfWeek;
    }

    private static TimeSpan GetTimeDifference(DateTime start, DateTime end) {
        if (end == null) {
            return new TimeSpan();
        }

        TimeSpan diff = end.Subtract(start);
        
        return diff;
    }

    private UserAccount GetUserByID(int userId) {
        return (from tl in this.db.TimeLog
                join ua in this.db.UserAccount on tl.UserId equals ua.Id
                where ua.Id == userId
                select ua).First();
    }
}
