using Microsoft.AspNetCore.Mvc.Rendering;

namespace ModernyzeWebsite.Models.TimeLog; 

public class TimeLogViewModel : SelectListItem {
    public string FullName { get; set; }

    public TimeSpan TimeLogged { get; set; }

    public string ReadableTimeLogged {
        get {
            return
                $"{this.TimeLogged.Hours} hours, {this.TimeLogged.Minutes} minutes, {this.TimeLogged.Seconds} seconds";
        }
    }
}
