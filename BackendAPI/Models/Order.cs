

#nullable disable
namespace Backend.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public DateTime CleaningDate { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}