namespace ModernyzeWebsite.Models;

public class AdminViewModel {
    public int UserId { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public bool NotVerified { get; set; }

    public bool IsAdmin { get; set; }
}
