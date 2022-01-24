using System.ComponentModel.DataAnnotations;

namespace ModernyzeWebsite.Models.FourUp; 

public class FourUp {

    [Key]
    public int Id { get; set; }

    [Required]
    [RegularExpression("https://docs.google.com/document/[^*]+", ErrorMessage = "Invalid Source URL.")]
    public string Source { get; set; }

    [Required]
    public DateTime WeekOf { get; set; }
}
