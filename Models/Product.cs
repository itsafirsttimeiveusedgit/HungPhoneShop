namespace HungPhoneShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Brand { get; set; }
        public decimal Price { get; set; }
        public required string Features { get; set; }
        public string? ImageUrl { get; set; } // Thêm trường ImageUrl
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}