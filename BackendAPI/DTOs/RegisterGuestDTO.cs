

using System.ComponentModel.DataAnnotations;

#nullable disable
public class GuestDTO
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
}