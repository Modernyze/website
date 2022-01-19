using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using ModernyzeWebsite.Data;
using ModernyzeWebsite.Models;
using ModernyzeWebsite.Models.TimeLog;
using ModernyzeWebsite.Models.User;

namespace ModernyzeWebsite.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly ModernyzeWebsiteContext db;

    public HomeController(ILogger<HomeController> logger, ModernyzeWebsiteContext db) {
        this._logger = logger;
        this.db = db;
    }

    public IActionResult Index() {
        return View(GetTimeLoggedByWeek(DateTime.Now));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier});
    }

    public IActionResult UpdateTimeLogged(string year, string week) {
        DateTime converted = ConvertWeekToDate(int.Parse(year), int.Parse(week));
        DateTime selectedDate = StartOfWeek(converted);
        return PartialView("Index", GetTimeLoggedByWeek(selectedDate));
    }

    private List<TimeLogViewModel> GetTimeLoggedByWeek(DateTime selectedDate) {
        DateTime startDate = StartOfWeek(selectedDate);
        DateTime endDate = startDate.AddDays(7);
        IEnumerable<TimeLog> list = this.db.TimeLog.Where(t => t.PunchInTime >= startDate && t.PunchInTime <= endDate)
                                        .ToList();
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

    /// <summary>
    ///     Get the Monday of the week the given date is in.
    /// </summary>
    /// <param name="selectedDate">The chosen date</param>
    /// <returns>A DateTime object at 12AM Monday in the same week as the given date.</returns>
    private static DateTime StartOfWeek(DateTime selectedDate) {
        int diff = (7 + (selectedDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime startOfWeek = selectedDate.AddDays(-1 * diff).Date;
        return startOfWeek;
    }

    private static DateTime ConvertWeekToDate(int year, int weekNum) {
        DateTime jan1 = new(year, 1, 1);
        int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

        // Use first Thursday in January to get first week of the year
        DateTime firstThursday = jan1.AddDays(daysOffset);
        Calendar cal = CultureInfo.CurrentCulture.Calendar;
        int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        int weekOfYear = weekNum;
        if (firstWeek == 1) {
            weekOfYear -= 1;
        }

        // using the first Thursday as the starting week ensures that we are starting in the right year.
        // then we add number of weeks multiplied with days
        return firstThursday.AddDays(weekOfYear * 7);
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
