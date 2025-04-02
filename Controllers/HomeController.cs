using Microsoft.AspNetCore.Mvc;
using HungPhoneShop.Models;
using Microsoft.EntityFrameworkCore;

namespace HungPhoneShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}