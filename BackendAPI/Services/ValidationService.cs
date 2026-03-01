

using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Services
{
    public class ValidationService
    {
        private readonly DataContext _context;
        public ValidationService(DataContext context)
        {
            _context = context;
        }

        //Admin Validation
        public async Task<bool> IsAdmin(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }
            return user.RoleId == 3;
        }

        //Product Validation
        public async Task<bool> CheckProductExists(string name)
        {
            return await _context.Products.AnyAsync(p => p.Name == name);
        }

    }
}