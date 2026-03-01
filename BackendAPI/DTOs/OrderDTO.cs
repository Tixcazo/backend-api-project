

#nullable disable

namespace Backend.DTOs
{
    public class OrderDTO
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime CleaningDate { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}
