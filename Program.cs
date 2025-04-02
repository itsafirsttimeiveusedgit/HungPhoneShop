using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HungPhoneShop.Models;

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
    }
    catch (Exception ex)
    {
        // Xử lý lỗi nếu có
        Console.WriteLine($"Error creating admin user: {ex.Message}");
    }
}

app.Run();