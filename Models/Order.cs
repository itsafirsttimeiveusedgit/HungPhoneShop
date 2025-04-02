namespace HungPhoneShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public required string Status { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>(); // Khởi tạo mặc định
    }
}