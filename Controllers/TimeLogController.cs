using System.Globalization;
using Microsoft.AspNetCore.Mvc;
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
    [HttpGet]
    public ActionResult Index() {
        return View();
    }

    // GET: TimeReport
    [HttpGet]
    public IActionResult GenerateTimeLoggedReport(int year, int week) {
        DateTime converted = ConvertWeekToDate(year, week);
        DateTime startDate = StartOfWeek(converted);
        DateTime endDate = startDate.AddDays(7);
        IEnumerable<TimeLog> rawLogs = GetTimeLogsForGivenDateRange(startDate, endDate).ToList();
        // Create a map between UserID and FullName for the Reporting feature to use.
        Dictionary<int, string> nameMap = new();
        foreach (TimeLog log in rawLogs) {
            if (nameMap.ContainsKey(log.UserId)) {
                continue;
            }

            string nameForUser = GetUserAccountByID(log.UserId).FullName;
            nameMap.Add(log.UserId, nameForUser);
        }
        return View("TimeReport", new TimeReport(rawLogs, startDate, nameMap));
    }

    #endregion

    #region POST Methods

    #endregion

    #endregion

    #region AJAX Methods

    // POST: Punch In
    [HttpPost]
    public async Task<IActionResult> PunchIn() {
        // If the session doesn't contain a UserId variable, the current user isn't logged in.
        if (string.IsNullOrEmpty(this.HttpContext.Session.GetString("UserId"))) {
            return Json(new {success = false, responseText = "You must be logged in to be able to record your time."});
        }

        int userID = int.Parse(this.HttpContext.Session.GetString("UserId"));
        UserAccount user = GetUserAccountByID(userID);
        // If we don't have an account for a given ID, there was a database
        // error or the current user was able to inject the UserId variable.
        if (user == null) {
            return Json(new {success = false, responseText = "An error occurred when trying to punch in."});
        }

        this.db.TimeLog.Add(new TimeLog {
            UserId = userID,
            PunchInTime = DateTime.Now,
            PunchOutTime = null
        });
        Task<int> save = this.db.SaveChangesAsync();
        save.Wait();
        int recordsAffected = save.Result;

        return Json(new {
            success = recordsAffected == 1,
            responseText = recordsAffected == 1 ? "Success" : "There was a problem punching in."
        });
    }

    // POST: Punch Out
    [HttpPost]
    public async Task<IActionResult> PunchOut() {
        // If the session doesn't contain a UserId variable, the current user isn't logged in.
        if (string.IsNullOrEmpty(this.HttpContext.Session.GetString("UserId"))) {
            return Json(new {success = false, responseText = "You must be logged in to be able to record your time."});
        }

        int userID = int.Parse(this.HttpContext.Session.GetString("UserId"));
        UserAccount user = GetUserAccountByID(userID);
        // If we don't have an account for a given ID, there was a database
        // error or the current user was able to inject the UserId variable.
        if (user == null) {
            return Json(new {success = false, responseText = "An error occurred when trying to punch out."});
        }

        TimeLog mostRecent;
        try {
            mostRecent = this.db.TimeLog.OrderByDescending(t => t.PunchInTime).First(t => t.UserId == user.Id);
        }
        catch (Exception) {
            return Json(new {success = false, responseText = "An error occurred when trying to punch out."});
        }

        mostRecent.PunchOutTime = DateTime.Now;
        Task<int> save = this.db.SaveChangesAsync();
        save.Wait();
        int recordsAffected = save.Result;

        return Json(new {
            success = recordsAffected == 1,
            responseText = recordsAffected == 1 ? "Success" : "There was a problem punching out."
        });
    }

    // POST: Record Meeting
    [HttpPost]
    public async Task<IActionResult> RecordMeeting(int hours, int minutes) {
        // If the session doesn't contain a UserId variable, the current user isn't logged in.
        if (string.IsNullOrEmpty(this.HttpContext.Session.GetString("UserId"))) {
            this.ViewBag.ErrorMessage = "You must be logged in to be able to record your time.";
            return Json(new {success = false, responseText = "You must be logged in to record time!"});
        }

        int userID = int.Parse(this.HttpContext.Session.GetString("UserId"));

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

    /// <summary>
    ///     Get the table of TimeLogs grouped by user for the week containing the selected date.
    /// </summary>
    public IActionResult GetTimeLoggedByWeek(DateTime selectedDate) {
        DateTime startDate = StartOfWeek(selectedDate);
        DateTime endDate = startDate.AddDays(7);
        IEnumerable<TimeLog> list = GetTimeLogsForGivenDateRange(startDate, endDate).ToList();
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

    public IActionResult UpdateTimeLogged(int year, int week) {
        DateTime converted = ConvertWeekToDate(year, week);
        DateTime selectedDate = StartOfWeek(converted);
        return GetTimeLoggedByWeek(selectedDate);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Get all TimeLogs within the given date range.
    /// </summary>
    /// <param name="start">Start date</param>
    /// <param name="end">End Date</param>
    /// <returns>A List of all TimeLogs in the database that were recorded
    /// between the given date range.</returns>
    private IEnumerable<TimeLog> GetTimeLogsForGivenDateRange(DateTime start, DateTime end) {
        return this.db.TimeLog.Where(t => t.PunchInTime >= start && t.PunchInTime <= end);
    }

    /// <summary>
    ///     Try to get a user account by the given user ID.
    /// </summary>
    /// <param name="userID">The user ID to search for.</param>
    /// <returns>
    ///     If the user exists, this method will return a UserAccount object.
    ///     If the user doesn't exist, this method returns null.
    /// </returns>
    private UserAccount? GetUserAccountByID(int userID) {
        UserAccount userAccount;
        try {
            userAccount = this.db.UserAccount.First(u => u.Id == userID);
        }
        catch (Exception) {
            return null;
        }

        return userAccount;
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
        if (end == DateTime.MinValue) {
            return new TimeSpan();
        }

        TimeSpan diff = end.Subtract(start);

        return diff;
    }

    #endregion
}
