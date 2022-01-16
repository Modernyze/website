using System.ComponentModel.DataAnnotations;

namespace ModernyzeWebsite.Models; 

public class UserAccount {
    [Key] public int Id { get; set; }

    [Required] [StringLength(50)] public string Username { get; set; }

    [Required] [StringLength(50)] public string FirstName { get; set; }

    [Required] [StringLength(50)] public string LastName { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}")]
    public string Email { get; set; }

    [Required] [StringLength(50)] public string Password { get; set; }
}
