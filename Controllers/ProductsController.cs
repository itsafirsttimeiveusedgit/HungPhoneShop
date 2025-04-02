using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HungPhoneShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HungPhoneShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Products
        public async Task<IActionResult> Index(string searchString, string brand, decimal? minPrice, decimal? maxPrice, string features, string sortOrder)
        {
            var products = from p in _context.Products.Include(p => p.Category)
                        select p;

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            // Lọc theo hãng
            if (!string.IsNullOrEmpty(brand))
            {
                products = products.Where(p => p.Brand == brand);
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // Lọc theo tính năng
            if (!string.IsNullOrEmpty(features))
            {
                products = products.Where(p => p.Features.Contains(features));
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            return View(await products.ToListAsync());
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: /Products/Create (Admin/Employee only)
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name"); // Hiển thị Id trong dropdown
            ViewBag.CategoryList = _context.Categories.ToList(); // Truyền danh sách danh mục để JavaScript sử dụng
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create(Product product)
        {

            if (product.Category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid Category.");
                return View(product);
            }

            // Lấy đối tượng Category dựa trên CategoryId
            product.Category = await _context.Categories.FindAsync(product.CategoryId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Id");
            ViewBag.CategoryList = _context.Categories.ToList();
            return View(product);
        }

        // GET: /Products/Edit/5 (Admin/Employee only)
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // GET: /Products/Delete/5 (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}