

using System.ComponentModel.DataAnnotations;

#nullable disable
public class LoginDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}