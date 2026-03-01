

using System.ComponentModel.DataAnnotations;


namespace Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        public DateTime? Birthdate { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        // กำหนดค่าเริ่มต้นของ isRegisteredUser เป็น false 
        // เพื่อให้แน่ใจว่าผู้ใช้ใหม่จะไม่ถูกพิจารณาว่าเป็นผู้ใช้ที่ลงทะเบียนแล้วจนกว่าจะมีการตั้งค่านี้เป็น true
        [Required]
        public bool isRegisteredUser { get; set; } = false;

        public ICollection<Order>? Orders { get; set; }

    }
}