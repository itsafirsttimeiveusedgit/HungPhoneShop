using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HungPhoneShop.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure Identity options (optional)
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tạo tài khoản Admin và các vai trò khi ứng dụng khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        // Tạo các vai trò nếu chưa tồn tại
        string[] roleNames = { "Admin", "Employee", "Customer", "Company" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Tạo tài khoản Admin nếu chưa tồn tại
        string adminEmail = "admin@hungphoneshop.com";
        string adminPassword = "Admin123"; // Mật khẩu phải đáp ứng yêu cầu (ít nhất 6 ký tự, có số, chữ thường, chữ hoa)

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                // Gán vai trò Admin cho tài khoản
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                // In ra lỗi nếu tạo tài khoản thất bại
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }

        // Seed Category
        string[] categories = { "Smartphone", "Feature Phone", "Tablet", "Accessory", "Laptop" };
        foreach (var categoryName in categories)
        {
            if (!await dbContext.Categories.AnyAsync(c => c.Name == categoryName))
            {
                dbContext.Categories.Add(new Category { Name = categoryName });
            }
        }

        await dbContext.SaveChangesAsync();

        // Seed Products with Real Data (Actual Product Names and Prices)
        var products = new List<(string Brand, string Model, decimal Price, string Features, string ImageUrl)>
        {
            // Apple
            ("Apple", "iPhone 16", 1099.99m, "iPhone 16 with A16 Bionic chip, 5G, 6.7-inch display", "image_url_iphone16"),
            ("Apple", "iPhone 15", 999.99m, "iPhone 15 with A15 Bionic chip, 5G, 6.1-inch display", "image_url_iphone15"),
            ("Apple", "iPhone 14 Pro", 1099.00m, "iPhone 14 Pro with Dynamic Island, 48MP camera", "image_url_iphone14pro"),
            ("Apple", "iPhone SE 2023", 429.00m, "iPhone SE with A15 Bionic chip, 5G", "image_url_iphonese2023"),
            ("Apple", "iPhone 13", 799.00m, "iPhone 13 with A15 Bionic chip, dual camera system", "image_url_iphone13"),

            // Samsung
            ("Samsung", "Galaxy S23 Ultra", 1199.99m, "Galaxy S23 Ultra with 200MP camera, Snapdragon 8 Gen 2", "image_url_s23ultra"),
            ("Samsung", "Galaxy S23", 799.99m, "Galaxy S23 with Snapdragon 8 Gen 2, 50MP camera", "image_url_s23"),
            ("Samsung", "Galaxy A54", 449.99m, "Galaxy A54 with 120Hz AMOLED display, 5G", "image_url_a54"),
            ("Samsung", "Galaxy Z Flip 5", 999.99m, "Galaxy Z Flip 5 with foldable display", "image_url_zflip5"),
            ("Samsung", "Galaxy A14", 249.99m, "Galaxy A14 with 6.6-inch display, 5G", "image_url_a14"),

            // Xiaomi
            ("Xiaomi", "Xiaomi 13 Pro", 999.99m, "Xiaomi 13 Pro with Leica cameras, Snapdragon 8 Gen 2", "image_url_xiaomi13pro"),
            ("Xiaomi", "Redmi Note 12", 199.99m, "Redmi Note 12 with 90Hz AMOLED, Snapdragon 680", "image_url_redminote12"),
            ("Xiaomi", "Xiaomi 12", 749.99m, "Xiaomi 12 with 50MP camera, Snapdragon 8 Gen 1", "image_url_xiaomi12"),
            ("Xiaomi", "Redmi K40", 399.99m, "Redmi K40 with Snapdragon 870, 120Hz AMOLED", "image_url_redmik40"),
            ("Xiaomi", "Mi 11 Ultra", 749.00m, "Mi 11 Ultra with 50MP camera, Snapdragon 888", "image_url_mi11ultra"),

            // Oppo
            ("Oppo", "Oppo Find X5 Pro", 1199.99m, "Oppo Find X5 Pro with 50MP camera, Snapdragon 8 Gen 1", "image_url_findx5pro"),
            ("Oppo", "Oppo Reno 8 Pro", 499.99m, "Oppo Reno 8 Pro with AMOLED display, MediaTek Dimensity 8100", "image_url_reno8pro"),
            ("Oppo", "Oppo A78", 199.99m, "Oppo A78 with 90Hz display, MediaTek Dimensity 700", "image_url_a78"),
            ("Oppo", "Oppo F21 Pro", 349.99m, "Oppo F21 Pro with 64MP camera, Snapdragon 680", "image_url_f21pro"),
            ("Oppo", "Oppo A16", 129.99m, "Oppo A16 with 13MP camera, 5000mAh battery", "image_url_a16"),

            // Realme
            ("Realme", "Realme GT 2 Pro", 749.99m, "Realme GT 2 Pro with Snapdragon 8 Gen 1, 50MP camera", "image_url_gt2pro"),
            ("Realme", "Realme 10", 299.99m, "Realme 10 with MediaTek Helio G99, 50MP camera", "image_url_realme10"),
            ("Realme", "Realme 9 Pro+", 349.99m, "Realme 9 Pro+ with 50MP camera, Snapdragon 695", "image_url_realme9pro"),
            ("Realme", "Realme GT", 499.99m, "Realme GT with Snapdragon 870, 120Hz AMOLED", "image_url_gt"),
            ("Realme", "Realme Narzo 50", 199.99m, "Realme Narzo 50 with MediaTek Helio G96, 5000mAh battery", "image_url_narzo50")
        };

        // Loop through each product and add it to the database
        foreach (var product in products)
        {
            var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Smartphone");
            if (category != null)
            {
                var productName = $"{product.Brand} {product.Model}";

                if (!await dbContext.Products.AnyAsync(p => p.Name == productName))
                {
                    dbContext.Products.Add(new Product
                    {
                        Name = productName,
                        Brand = product.Brand,
                        Price = product.Price, // No need to cast, already in decimal
                        Features = product.Features,
                        ImageUrl = product.ImageUrl,
                        CategoryId = category.Id
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();


        await dbContext.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        // Xử lý lỗi nếu có
        Console.WriteLine($"Error seeding data: {ex.Message}");
    }
}

app.Run();
