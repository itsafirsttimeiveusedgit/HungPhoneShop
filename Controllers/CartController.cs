using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HungPhoneShop.Models;
using Microsoft.EntityFrameworkCore;

namespace HungPhoneShop.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
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
            return View(cartItems);
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem == null)
            {
                // Lấy Product từ cơ sở dữ liệu
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Product = product, // Gán giá trị cho Product (bắt buộc vì có từ khóa required)
                    Quantity = quantity
                };
                _context.Carts.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
                _context.Carts.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var cartItem = await _context.Carts.FindAsync(id);
            if (cartItem != null && cartItem.UserId == userId)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}