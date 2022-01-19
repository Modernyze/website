using System.Globalization;
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

    #region Endpoint Methods

    #region GET Methods

    // GET: TimeLog
    public async Task<IActionResult> Index() {
        return View(await this.db.TimeLog.ToListAsync());
    }

    #endregion

    #region POST Methods

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

    // POST: Record Meeting
    [HttpPost]
    public async Task<IActionResult> RecordMeeting(int hours, int minutes) {
        int userID = 0;
        try {
            userID = int.Parse(this.HttpContext.Session.GetString("UserId"));
        }
        catch (ArgumentNullException) {
            return Json(new {success = false, responseText = "You must be logged in to record time!"});
        }

        DateTime now = DateTime.Now;
        DateTime start = now.Subtract(new TimeSpan(hours, minutes, 0));
        TimeLog meeting = new() {
            UserId = userID,
            PunchInTime = start,
            PunchOutTime = now
        };
        await this.db.TimeLog.AddAsync(meeting);
        Task<int> save = this.db.SaveChangesAsync();
        save.Wait();
        // We only expect one record to be created.
        // If this is not true, tell the user there was an error.
        return Json(new {
            success = save.Result == 1,
            responseText = save.Result == 1 ? "Success" : "There was a problem recording the meeting."
        });
    }

    #endregion

    #endregion

    #region AJAX Methods
    
    /// <summary>
    /// Get the table of TimeLogs grouped by user for the week containing the selected date.
    /// </summary>
    public IActionResult GetTimeLoggedByWeek(DateTime selectedDate) {
        DateTime startDate = StartOfWeek(selectedDate);
        DateTime endDate = startDate.AddDays(7);
        IEnumerable<TimeLog> list = this.db.TimeLog.Where(t => t.PunchInTime >= startDate && t.PunchInTime <= endDate)
                                        .ToList();
        Dictionary<int, TimeLogViewModel> timePerUser = new();
        foreach (TimeLog timeLog in list) {
            if (timePerUser.ContainsKey(timeLog.UserId)) {
                TimeLogViewModel current = timePerUser[timeLog.UserId];
                TimeSpan sum =
                    current.TimeLogged.Add(
                        GetTimeDifference(timeLog.PunchInTime, timeLog.PunchOutTime.GetValueOrDefault()));
                current.TimeLogged = sum;
                continue;
            }

            timePerUser.Add(timeLog.UserId, new TimeLogViewModel {
                FullName = GetUserAccountByID(timeLog.UserId).FullName,
                TimeLogged = GetTimeDifference(timeLog.PunchInTime, timeLog.PunchOutTime.GetValueOrDefault())
            });
        }

        return PartialView("Weekly", timePerUser.Values.ToList());
    }
    
    public IActionResult UpdateTimeLogged(string year, string week) {
        DateTime converted = ConvertWeekToDate(int.Parse(year), int.Parse(week));
        DateTime selectedDate = StartOfWeek(converted);
        return GetTimeLoggedByWeek(selectedDate);
    }

    #endregion

    #region Private Helper Methods

    private UserAccount GetUserAccountByID(int userID) {
        return this.db.UserAccount.Where(u => u.Id == userID).FirstOrDefault();
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

    #endregion
}
