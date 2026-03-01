using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Backend.Models;
using Backend.Services;
using Backend.Data;
using Backend.DTOs;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ValidationService _validationService;

        public OrderController(DataContext dataContext, ValidationService validationService)
        {
            _dataContext = dataContext;
            _validationService = validationService;
        }

        /// <summary>
        /// Get all orders (Admin sees all, Users see only their own)
        /// </summary>
        // นี่คือเมธอด GetAllOrders ที่จะดึงข้อมูลคำสั่งซื้อทั้งหมดจากฐานข้อมูลและส่งกลับไปยังผู้ใช้ในรูปแบบ JSON
        // โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถดูคำสั่งซื้อทั้งหมดได้ 
        // ส่วนผู้ใช้ทั่วไปจะเห็นเฉพาะคำสั่งซื้อของตัวเองเท่านั้น
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            // รับ user id จาก JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (_dataContext.Orders == null)
            {
                return NotFound("No orders found.");
            }

            // ตรวจสอบว่าเป็น Admin หรือไม่
            if (await _validationService.IsAdmin(userId))
            {
                // Admin สามารถดู order ทั้งหมดได้
                return Ok(await _dataContext.Orders
                    .Include(o => o.User)
                    .Include(o => o.Product)
                    .ToListAsync());
            }
            else
            {
                // User ทั่วไปสามารถดูเฉพาะ order ของตัวเอง
                return Ok(await _dataContext.Orders
                    .Include(o => o.User)
                    .Include(o => o.Product)
                    .Where(o => o.UserId == userId)
                    .ToListAsync());
            }
        }

        /// <summary>
        /// Get order by ID (Admin can access any, User can access only their own)
        /// </summary>
        // นี่คือเมธอด GetOrderById ที่จะรับ ID ของคำสั่งซื้อเป็นพารามิเตอร์และดึงข้อมูลคำสั่งซื้อนั้นจากฐานข้อมูล
        // ถ้าคำสั่งซื้อถูกพบ จะส่งกลับข้อมูลคำสั่งซื้อในรูปแบบ JSON ถ้าไม่พบ จะส่งกลับข้อความว่าไม่พบคำสั่งซื้อ
        // โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถดูคำสั่งซื้อทั้งหมดได้ 
        // ส่วนผู้ใช้ทั่วไปจะเห็นเฉพาะคำสั่งซื้อของตัวเองเท่านั้น
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            // รับ user id จาก JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (_dataContext.Orders == null)
            {
                return NotFound("No orders found.");
            }

            var order = await _dataContext.Orders
                .Include(o => o.User)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            // ตรวจสอบสิทธิ์ในการเข้าถึง
            if (!await _validationService.IsAdmin(userId) && order.UserId != userId)
            {
                return BadRequest("You do not have permission to view this order.");
            }

            return Ok(order);
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "productId": 1,
        ///         "cleaningDate": "2026-03-15T10:00:00",
        ///         "description": "Deep cleaning service",
        ///         "price": 2500.00
        ///     }
        /// 
        /// </remarks>
        // นี่คือเมธอด CreateOrder ที่จะรับข้อมูลคำสั่งซื้อใหม่จากผู้ใช้ในรูปแบบ JSON และเพิ่มข้อมูลคำสั่งซื้อนั้นลงในฐานข้อมูล
        // โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้ ส่วนผู้ใช้ทั่วไปจะสามารถสร้างคำสั่งซื้อของตัวเองได้
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder(OrderDTO orderDto)
        {
            // รับ user id จาก JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            // ตรวจสอบว่า Product มีอยู่หรือไม่
            var product = await _dataContext.Products.FindAsync(orderDto.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {orderDto.ProductId} not found.");
            }

            // สร้าง Order ใหม่
            var order = new Order
            {
                UserId = userId,
                ProductId = orderDto.ProductId,
                CleaningDate = orderDto.CleaningDate,
                Description = orderDto.Description,
                Price = orderDto.Price
            };

            _dataContext.Orders.Add(order);
            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Internal server error: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        /// <summary>
        /// Update an existing order (Admin can update any, User can update only their own)
        /// </summary>
        // นี่คือเมธอด UpdateOrder ที่จะรับ ID ของคำสั่งซื้อที่ต้องการอัปเดตและข้อมูลคำสั่งซื้อใหม่จากผู้ใช้ในรูปแบบ 
        // JSON และอัปเดตข้อมูลคำสั่งซื้อนั้นในฐานข้อมูล
        // โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถอัปเดตคำสั่งซื้อทั้งหมดได้ 
        // ส่วนผู้ใช้ทั่วไปจะสามารถอัปเดตเฉพาะคำสั่งซื้อของตัวเองได้
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrder(int id, OrderDTO orderDto)
        {
            // รับ user id จาก JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (orderDto.Id.HasValue && id != orderDto.Id.Value)
            {
                return BadRequest("Order ID mismatch.");
            }

            var existingOrder = await _dataContext.Orders.FindAsync(id);
            if (existingOrder == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            // ตรวจสอบสิทธิ์ในการแก้ไข
            if (!await _validationService.IsAdmin(userId) && existingOrder.UserId != userId)
            {
                return BadRequest("You do not have permission to update this order.");
            }

            // ตรวจสอบว่า Product มีอยู่หรือไม่
            var product = await _dataContext.Products.FindAsync(orderDto.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {orderDto.ProductId} not found.");
            }

            // อัปเดตข้อมูล
            existingOrder.ProductId = orderDto.ProductId;
            existingOrder.CleaningDate = orderDto.CleaningDate;
            existingOrder.Description = orderDto.Description;
            existingOrder.Price = orderDto.Price;

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
        /// Delete an order (Admin can delete any, User can delete only their own)
        /// </summary>
        // นี่คือเมธอด DeleteOrder ที่จะรับ ID ของคำสั่งซื้อที่ต้องการลบจากผู้ใช้และลบคำสั่งซื้อนั้นออกจากฐานข้อมูล
        // โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถลบคำสั่งซื้อทั้งหมดได้
        // ส่วนผู้ใช้ทั่วไปจะสามารถลบเฉพาะคำสั่งซื้อของตัวเองได้
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            // รับ user id จาก JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            var order = await _dataContext.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            // ตรวจสอบสิทธิ์ในการลบ
            if (!await _validationService.IsAdmin(userId) && order.UserId != userId)
            {
                return BadRequest("You do not have permission to delete this order.");
            }

            _dataContext.Orders.Remove(order);
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