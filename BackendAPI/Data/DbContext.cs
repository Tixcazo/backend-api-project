

using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
            // Ensure database is created
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // One to Many relationship between User and Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User) // หมายความว่า Order มี User หนึ่งคนเป็นเจ้าของ
                .WithMany(u => u.Orders) // หมายความว่า User หนึ่งคนสามารถมีหลาย Order ได้
                .HasForeignKey(o => o.UserId); // กำหนด Foreign Key ว่าคือฟิลด์ UserId ใน Order

            // One to Many relationship between User and Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role) // หมายความว่า User แต่ละคนมี Role หนึ่งตำแหน่ง
                .WithMany(r => r.Users) // หมายความว่า Role หนึ่งตำแหน่งสามารถมีหลาย User ได้
                .HasForeignKey(u => u.RoleId); // กำหนด Foreign Key ว่าคือฟิลด์ RoleId ใน User

            // One to Many relationship between Product and Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product) // หมายความว่า Order แต่ละออเดอร์มี Product หนึ่งชิ้น
                .WithMany(p => p.Orders) // หมายความว่า Product หนึ่งชิ้นสามารถอยู่ในหลาย Order ได้
                .HasForeignKey(o => o.ProductId); // กำหนด Foreign Key ว่าคือฟิลด์ ProductId ใน Order
        }
    }
}