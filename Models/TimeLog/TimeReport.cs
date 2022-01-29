using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModernyzeWebsite.Models.TimeLog;

public class TimeReport {
    private readonly IEnumerable<TimeLog> logs;
    private readonly Dictionary<int, string> nameMap;
    private readonly DateTime startDate;

    public TimeReport(IEnumerable<TimeLog> timeLogs, DateTime startDate, Dictionary<int, string> nameMap) {
        this.logs = timeLogs;
        this.startDate = startDate;
        this.nameMap = nameMap;
    }

    [NotMapped]
    public string ReportTitle {
        get { return $"Week of {this.startDate.Month}/{this.startDate.Day}/{this.startDate.Year} Time Report"; }
    }

    [NotMapped]
    public string IndividualReport {
        get {
            StringBuilder sb = new();
            foreach (KeyValuePair<int, TimeSpan> log in TotalTimeByMember()) {
                string name = this.nameMap[log.Key];
                sb.Append("{label:'").Append($"{name}").Append("', y:").Append(log.Value.TotalHours).Append("},");
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }

    [NotMapped]
    public string TeamReport {
        get {
            TimeSpan total = TotalTimeByTeam();
            return "{label:'Team Modernyze', y:" + total.TotalHours + "}";
        }
    }

    #region Private Helper Methods
    
    /// <summary>
    /// Group all logs together by user, and sum the time logged.
    /// </summary>
    /// <returns>A Dictionary, with the keys being UserIDs and values
    /// being sum of time logged by the user.</returns>
    private Dictionary<int, TimeSpan> TotalTimeByMember() {
        Dictionary<int, TimeSpan> byMember = new();
        foreach (TimeLog log in this.logs) {
            if (log.PunchOutTime == null) {
                continue;
            }

            if (byMember.ContainsKey(log.UserId)) {
                TimeSpan current = byMember[log.UserId];
                current += log.PunchOutTime.Value.Subtract(log.PunchInTime);
                byMember[log.UserId] = current;
                continue;
            }

            byMember.Add(log.UserId, log.PunchOutTime.Value.Subtract(log.PunchInTime));
        }

        return byMember;
    }

    /// <summary>
    /// Sum all time logged by the team.
    /// </summary>
    /// <returns>A TimeSpan object representing the sum of all logged time.</returns>
    private TimeSpan TotalTimeByTeam() {
        TimeSpan sum = new(0);
        foreach (TimeLog log in this.logs) {
            if (log.PunchOutTime == null) {
                continue;
            }

            sum += log.PunchOutTime.Value.Subtract(log.PunchInTime);
        }

        return sum;
    }

    #endregion
}
