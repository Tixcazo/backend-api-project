

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Data;
using Backend.Models;
using Backend.Config;

#nullable disable
public class AuthService
{
    private readonly DataContext _dataContext;
    private readonly JwtSettings _jwtSettings;
    public AuthService(DataContext dataContext, JwtSettings jwtSettings)
    {
        _dataContext = dataContext;
        _jwtSettings = jwtSettings;
    }

    //เราเริ่มต้นด้วยการสร้างเมธอด ValidateUser ซึ่งรับอีเมลและรหัสผ่านเป็นพารามิเตอร์ 
    //จากนั้นเราค้นหาผู้ใช้ในฐานข้อมูลโดยใช้อีเมลที่ให้มา ถ้าไม่พบผู้ใช้หรือผู้ใช้ไม่ได้ลงทะเบียนหรือไม่มีรหัสผ่านที่ถูกแฮช เราจะคืนค่า false
    public async Task<bool> ValidateUser(string email, string password)
    {
        //เราใข้ SingleOrDefaultAsync เพื่อค้นหาผู้ใช้ที่มีอีเมลตรงกับที่ให้มา ถ้าไม่พบผู้ใช้จะคืนค่า null
        //SingleOrDefaultAsync คือเมธอดที่ใช้ในการค้นหาข้อมูลในฐานข้อมูลแบบอะซิงโครนัส 
        //โดยจะคืนค่าผลลัพธ์เป็นอ็อบเจ็กต์เดียวหรือค่า null ถ้าไม่พบข้อมูลที่ตรงกับเงื่อนไขที่กำหนด
        var user = await _dataContext.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            return false;
        }

        if (!user.isRegisteredUser)
        {
            return false;
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return false;
        }

        //เราใช้ PasswordHasher เพื่อเปรียบเทียบรหัสผ่านที่ให้มากับรหัสผ่านที่ถูกแฮชในฐานข้อมูล 
        //โดยใช้ VerifyHashedPassword ซึ่งจะคืนค่าเป็น PasswordVerificationResult
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Success;
    }

    //เราสร่างเมธอด RegisteredUser เพื่อรับข้อมูลผู้ใช้ใหม่และเพิ่มลงในฐานข้อมูล 
    //โดยจะตรวจสอบว่าผู้ใช้ที่มีอีเมลนี้มีอยู่แล้วหรือไม่ ถ้ามีอยู่แล้วจะคืนค่า false
    public async Task<bool> RegisteredUser(
        string email,
        string password,
        string firstName,
        string lastName,
        string address,
        string phoneNumber,
        DateTime birthdate
    )
    {
        //ตรวจสอบว่าผู้ใช้ที่มีอีเมลนี้มีอยู่แล้วหรือไม่ ถ้ามีอยู่แล้วจะคืนค่า false
        var existingUser = await _dataContext.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            return false;
        }

        //แฮชรหัสผ่านโดยใช้ PasswordHasher และสร้างอ็อบเจ็กต์ User ใหม่ จากนั้นเพิ่มผู้ใช้ใหม่ลงในฐานข้อมูลและบันทึกการเปลี่ยนแปลง
        var passwordHasher = new PasswordHasher<User>();
        var hashedPassword = passwordHasher.HashPassword(null, password);

        //สร้าง User ใหม่และกำหนดค่าต่างๆ
        var createUser = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Address = address,
            PhoneNumber = phoneNumber,
            Birthdate = birthdate,
            PasswordHash = hashedPassword,
            RoleId = 1, // กำหนด RoleId เริ่มต้นเป็น 1 (สามารถปรับตามความต้องการ)
            isRegisteredUser = true, // กำหนดให้เป็นผู้ใช้ที่ลงทะเบียนแล้ว
        };

        //เพิ่มผู้ใช้ใหม่ลงในฐานข้อมูลและบันทึกการเปลี่ยนแปลง
        _dataContext.Users.Add(createUser); // Add คือเมธอดที่ใช้ในการเพิ่มอ็อบเจ็กต์ใหม่ลงในฐานข้อมูล
        await _dataContext.SaveChangesAsync(); // SaveChangesAsync คือเมธอดที่ใช้ในการบันทึกการเปลี่ยนแปลงที่เกิดขึ้นในฐานข้อมูลแบบอะซิงโครนัส

        return true;
    }

    //เราสร้างเมธอด RegisteredGuest เพื่อรับข้อมูลผู้ใช้แบบ Guest และเพิ่มลงในฐานข้อมูล
    public async Task<User> RegisteredGuest(
        string email,
        string firstName,
        string lastName,
        string address,
        string phoneNumber
    )
    {
        //ตรวจสอบว่าผู้ใช้ที่มีอีเมลนี้มีอยู่แล้วหรือไม่ ถ้ามีอยู่แล้วจะคืนค่า null
        var existingUser = await _dataContext.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
        {
            return null;
        }

        //สร้าง User ใหม่และกำหนดค่าต่างๆ โดยไม่ต้องแฮชรหัสผ่านเพราะเป็นผู้ใช้แบบ Guest
        var createGuestUser = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Address = address,
            PhoneNumber = phoneNumber,
            PasswordHash = null,
            Birthdate = null,
            RoleId = 2, // กำหนด RoleId สำหรับ Guest เป็น 2 (สามารถปรับตามความต้องการ)
            isRegisteredUser = false // กำหนดให้เป็นผู้ใช้ที่ยังไม่ได้ลงทะเบียน
        };

        //เพิ่มผู้ใช้ใหม่ลงในฐานข้อมูลและบันทึกการเปลี่ยนแปลง
        _dataContext.Users.Add(createGuestUser);
        await _dataContext.SaveChangesAsync();

        return createGuestUser;
    }

    //เราสร้างเมธอด GetUserByEmail เพื่อค้นหาผู้ใช้โดยใช้อีเมล
    public async Task<User> GetUserByEmail(string email)
    {
        return await _dataContext.Users.SingleOrDefaultAsync(u => u.Email == email);
    }

    //เราสร้างเมธอด GenerateJwtToken เพื่อสร้างโทเค็น JWT สำหรับผู้ใช้ที่ได้รับการตรวจสอบแล้ว 
    //โดยใช้ข้อมูลจาก JwtSettings ในการกำหนดค่าโทเค็น
    public string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //ที่เราต้องหา Role ของผู้ใช้เพื่อเพิ่มเป็น Claim ในโทเค็น เราจะใช้ RoleId ของผู้ใช้เพื่อค้นหา Role 
        //ในฐานข้อมูล และนำชื่อ Role มาใช้ใน Claim
        var role = _dataContext.Roles.Find(user.RoleId);
        var roleName = role?.Name ?? "User";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, roleName)
        };
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

        //อธิบายโค้ดส่วนนี้:
        //1. เราสร้าง SymmetricSecurityKey โดยใช้ SecretKey จาก JwtSettings เพื่อใช้ในการเข้ารหัสโทเค็น
        //2. เราสร้าง SigningCredentials โดยใช้คีย์และอัลกอริทึม HmacSha256 เพื่อใช้ในการเซ็นโทเค็น
        //3. เราสร้าง Claims ซึ่งเป็นข้อมูลที่เราต้องการเก็บในโทเค็น ในที่นี้เรากำหนดให้มีอีเมลของผู้ใช้และ Jti (JWT ID) ซึ่งเป็นค่าเฉพาะสำหรับแต่ละโทเค็น
        //4. เราสร้าง JwtSecurityToken โดยกำหนด issuer, audience, claims, expiration time และ signing credentials
        //5. สุดท้ายเราสร้างโทเค็นในรูปแบบสตริงโดยใช้ JwtSecurityTokenHandler และคืนค่าโทเค็นนั้นกลับไป
    }

}