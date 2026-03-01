


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Backend.Models;
using Backend.Services;
using Backend.Data;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class RoleController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ValidationService _validationService;

        public RoleController(DataContext dataContext, ValidationService validationService)
        {
            _dataContext = dataContext;
            _validationService = validationService;
        }

        /// <summary>
        /// Get all roles (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize]
        //นี่คือเมธอด GetAllRoles ที่จะดึงข้อมูลบทบาททั้งหมดจากฐานข้อมูลและส่งกลับไปยังผู้ใช้ในรูปแบบ JSON 
        //โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to view roles.");
            }

            if (_dataContext.Roles == null)
            {
                return NotFound("No roles found.");
            }
            return Ok(await _dataContext.Roles.ToListAsync());
        }

        /// <summary>
        /// Get role by ID (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Role>> GetRoleById(int id)
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to view roles.");
            }
            if (_dataContext.Roles == null)
            {
                return NotFound("No roles found.");
            }
            var role = await _dataContext.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }
            return Ok(role);
        }

        /// <summary>
        /// Add a new role (Admin only)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///        "id": 0,
        ///        "name": "New Role"
        ///     }
        /// 
        /// </remarks>
        /// นี่คือเมธอด AddRole ที่จะรับข้อมูลบทบาทใหม่จากผู้ใช้ในรูปแบบ JSON และเพิ่มข้อมูลบทบาทนั้นลงในฐานข้อมูล
        /// โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Role>> AddRole(Role role)
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to add roles.");
            }

            _dataContext.Roles.Add(role);
            await _dataContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }


        /// <summary>
        /// Update a role (Admin only)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "id": 1,
        ///         "name": "Updated Role"
        ///     }
        /// 
        /// </remarks>
        /// นี่คือเมธอด UpdateRole ที่จะรับข้อมูลบทบาทที่ต้องการอัปเดตจากผู้ใช้ในรูปแบบ JSON และอัปเดตข้อมูลบทบาทนั้นในฐานข้อมูล
        /// โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to update roles.");
            }

            if (id != role.Id)
            {
                return BadRequest("Role ID mismatch.");
            }

            var existingRole = await _dataContext.Roles.FindAsync(id);
            if (existingRole == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            existingRole.Name = role.Name;

            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Internal server error: {ex.Message}");
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a role by ID (Admin only)
        /// </summary>
        // นี่คือเมธอด DeleteRole ที่จะรับ ID ของบทบาทที่ต้องการลบจากผู้ใช้และลบบทบาทนั้นออกจากฐานข้อมูล
        // โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRole(int id)
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to delete roles.");
            }

            var role = await _dataContext.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            _dataContext.Roles.Remove(role);
            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Internal server error: {ex.Message}");
            }

            return NoContent();
        }
    }
}