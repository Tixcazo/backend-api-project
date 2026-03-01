

using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        //เราดึง AuthService เข้ามาใช้ใน AuthController ผ่านการ Dependency Injection 
        //เพื่อให้เราสามารถเรียกใช้เมธอดต่างๆ ใน AuthService ได้
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// User login and return Token
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        /// 
        ///       "email": "user@example.com",
        ///       "password": "1234"
        /// 
        ///     }
        /// 
        /// </remarks>
        //เราสร้างเมธอด Login เพื่อรับข้อมูลการเข้าสู่ระบบของผู้ใช้และตรวจสอบความถูกต้องของข้อมูลนั้น 
        //โดยใช้เมธอด ValidateUser จาก AuthService ถ้าข้อมูลถูกต้อง เราจะสร้าง JWT Token และส่งกลับไปยังผู้ใช้
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            if (await _authService.ValidateUser(request.Email, request.Password))
            {
                var user = await _authService.GetUserByEmail(request.Email);
                var token = _authService.GenerateJwtToken(user);

                return Ok(new { Token = token });
            }
            return Unauthorized("Invalid email or password");
        }

        /// <summary>
        /// User registration
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     {
        ///     
        ///       "email": "user@example.com",
        ///       "password": "1234",
        ///       "firstName": "John",
        ///       "lastName": "Doe",
        ///       "address": "123 Main St",
        ///       "phoneNumber": "123-456-7890",
        ///       "birthdate": "1990-01-01"
        /// 
        ///     }
        /// 
        /// </remarks>
        // เราสร้างเมธอด Register เพื่อรับข้อมูลการลงทะเบียนของผู้ใช้และตรวจสอบว่าผู้ใช้ที่มีอีเมลเดียวกันมีอยู่แล้วหรือไม่ 
        // โดยใช้เมธอด RegisteredUser จาก AuthService ถ้าผู้ใช้ยังไม่มีอยู่ เราจะสร้างบัญชีผู้ใช้ใหม่และส่งกลับข้อความยืนยันการลงทะเบียน
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO request)
        {
            if (await _authService.RegisteredUser(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Address,
                request.PhoneNumber,
                request.Birthdate
            ))
            {
                return Ok($"User registered successfully with email: {request.Email}");
            }
            return BadRequest("Email already exists");
        }

        /// <summary>
        /// Guest user registration
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        /// 
        ///       "email": "guest@example.com",
        ///       "firstName": "Guest",
        ///       "lastName": "User",
        ///       "address": "456 Main St",
        ///       "phoneNumber": "987-654-3210"
        /// 
        ///     }
        /// 
        /// </remarks>
        /// เราสร้างเมธอด GuestRegister เพื่อรับข้อมูลการลงทะเบียนของผู้ใช้แบบ Guest และตรวจสอบว่าผู้ใช้ที่มีอีเมลเดียวกันมีอยู่แล้วหรือไม่ 
        /// โดยใช้เมธอด RegisteredGuest จาก AuthService ถ้าผู้ใช้ยังไม่มีอยู่ 
        /// เราจะสร้างบัญชีผู้ใช้แบบ Guest ใหม่และส่งกลับข้อความยืนยันการลงทะเบียน
        /// และ Guest จะไม่ได้รับ JWT Token เพราะเป็นผู้ใช้แบบ Guest ที่ไม่ต้องการเข้าสู่ระบบ แต่สามารถใช้บัญชีนี้ในการสั่งซื้อสินค้าได้
        [HttpPost("guest")]
        public async Task<IActionResult> GuestRegister([FromBody] GuestDTO request)
        {
            var guestUser = await _authService.RegisteredGuest(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Address,
                request.PhoneNumber
            );

            if (guestUser != null)
            {
                return Ok("Guest user registered successfully");
            }
            return BadRequest("Email already exists");
        }
    }
}