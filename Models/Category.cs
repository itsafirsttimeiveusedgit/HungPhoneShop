namespace HungPhoneShop.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Product> Products { get; set; } = new List<Product>(); // Khởi tạo mặc định
    }
}