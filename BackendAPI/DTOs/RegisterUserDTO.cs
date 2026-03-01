

using System.ComponentModel.DataAnnotations;

#nullable disable
public class RegisterDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    public string Address { get; set; }

    public string PhoneNumber { get; set; }

    public DateTime Birthdate { get; set; }
}