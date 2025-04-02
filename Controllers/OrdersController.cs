using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HungPhoneShop.Models;
using Microsoft.EntityFrameworkCore;

namespace HungPhoneShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Orders (Employee/Admin only)
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();
            return View(orders);
        }

        // GET: /Orders/Details/5 (Employee/Admin only)
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: /Orders/Checkout (Customer only)
        [Authorize(Roles = "Customer")]
        public IActionResult Checkout()
        {
            return View();
        }

        // POST: /Orders/Checkout (Customer only)
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutConfirmed()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // Create a new order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderDetails = new List<OrderDetail>() // Khởi tạo danh sách rỗng trước
            };

            // Tạo danh sách OrderDetails và gán Order cho từng OrderDetail
            var orderDetails = cartItems.Select(c => new OrderDetail
            {
                ProductId = c.ProductId,
                Product = c.Product,
                Quantity = c.Quantity,
                Price = c.Product.Price,
                Order = order // Gán Order cho OrderDetail (bắt buộc vì có từ khóa required)
            }).ToList();

            // Gán danh sách OrderDetails vào Order
            order.OrderDetails = orderDetails;

            // Add the order to the database
            _context.Orders.Add(order);

            // Remove items from the cart
            _context.Carts.RemoveRange(cartItems);

            // Save changes
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // GET: /Orders/OrderConfirmation/5 (Customer only)
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /Orders/UpdateStatus/5 (Employee only)
        [Authorize(Roles = "Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}