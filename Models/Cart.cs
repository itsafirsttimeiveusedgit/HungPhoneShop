namespace HungPhoneShop.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public int ProductId { get; set; }
        public required Product Product { get; set; }
        public int Quantity { get; set; }
    }
}