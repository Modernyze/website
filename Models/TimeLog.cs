namespace ModernyzeWebsite.Models; 

public class TimeLog {
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime PunchInTime { get; set; }

    public DateTime? PunchOutTime { get; set; }
}
