

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
    public class ProductController : ControllerBase
    {
        // เราดึง DataContext และ ValidationService เข้ามาใช้ใน ProductController ผ่านการ Dependency Injection
        private readonly DataContext _dataContext;
        private readonly ValidationService _validationService;

        public ProductController(DataContext dataContext, ValidationService validationService)
        {
            _dataContext = dataContext;
            _validationService = validationService;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        //นี่คือเมธอด GetAllProducts ที่จะดึงข้อมูลสินค้าทั้งหมดจากฐานข้อมูลและส่งกลับไปยังผู้ใช้ในรูปแบบ 
        //JSON โดยใช้ Entity Framework Core เพื่อเข้าถึงข้อมูลจาก DataContext
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            if (_dataContext.Products == null)
            {
                return NotFound("No products found.");
            }
            return Ok(await _dataContext.Products.ToListAsync());
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        //นี่คือเมธอด GetProductById ที่จะรับ ID ของสินค้าเป็นพารามิเตอร์และดึงข้อมูลสินค้านั้นจากฐานข้อมูล 
        //ถ้าสินค้าถูกพบ จะส่งกลับข้อมูลสินค้าในรูปแบบ JSON ถ้าไม่พบ จะส่งกลับข้อความว่าไม่พบสินค้า
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            if (_dataContext.Products == null)
            {
                return NotFound("No products found.");
            }

            var product = await _dataContext.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            return Ok(product);
        }


        /// <summary>
        /// Add a new product (Admin only)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "id": 0,
        ///         "name": "Rundtvask",
        ///         "description": "Overall vask",
        ///         "price": 4500.00
        ///     }
        /// 
        /// </remarks>
        //นี่คือเมธอด addProduct ที่จะรับข้อมูลสินค้าใหม่จากผู้ใช้ในรูปแบบ JSON และเพิ่มข้อมูลสินค้านั้นลงในฐานข้อมูล
        //โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> addProduct(Product addProduct)
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to add products.");
            }

            if (await _validationService.CheckProductExists(addProduct.Name))
            {
                return BadRequest("Product with the same name already exists.");
            }

            _dataContext.Products.Add(addProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Internal server error: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetProductById), new { id = addProduct.Id }, addProduct);
        }


        /// <summary>
        /// Update an existing product by ID (Admin only)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     {
        ///         "id": 1,
        ///         "name": "Update Rundtvask",
        ///         "description": "UpOverall vask",
        ///         "price": 2500.00
        ///     }
        /// 
        /// </remarks>
        //นี่คือเมธอด UpdateProduct ที่จะรับ ID ของสินค้าที่ต้องการ
        //อัพเดตและข้อมูลสินค้าใหม่จากผู้ใช้ในรูปแบบ JSON และอัพเดตข้อมูลสินค้านั้นในฐานข้อมูล 
        //โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, Product productDto)
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to update products.");
            }

            if (id != productDto.Id)
            {
                return BadRequest("Product ID mismatch.");
            }

            var existingProduct = await _dataContext.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;

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
        /// Delete a product by ID (Admin only)
        /// </summary>
        //นี่คือเมธอด DeleteProduct ที่จะรับ ID ของสินค้าที่ต้องการลบและลบสินค้านั้นจากฐานข้อมูล 
        //โดยเมธอดนี้จะถูกจำกัดให้เฉพาะผู้ใช้ที่มีบทบาทเป็น Admin เท่านั้นที่สามารถเข้าถึงได้
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // ตรวจสอบว่าผู้ใช้เป็น Admin หรือไม่
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Invalid user token.");
            }

            if (!await _validationService.IsAdmin(userId))
            {
                return BadRequest("You do not have permission to delete products.");
            }

            var product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            _dataContext.Products.Remove(product);
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