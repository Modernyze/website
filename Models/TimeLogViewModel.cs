using Microsoft.AspNetCore.Mvc.Rendering;

namespace ModernyzeWebsite.Models
{
    public class TimeLogViewModel : SelectListItem
    {
        public string FullName { get; set; }

        public TimeSpan TimeLogged { get; set; }
    }
}
