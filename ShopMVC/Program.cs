using Microsoft.EntityFrameworkCore;
using ShopMVC.Data;
using ShopMVC.Services.Interfaces;
using ShopMVC.Services.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);



// 📦 DbContext - Đọc connection string từ appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PhoneShopDB")));

// 🎮 Controllers with Views
builder.Services.AddControllersWithViews();

// 🔐 Session Configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi không có quyền
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Cookie tồn tại 30 ngày
    });
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout 30 phút
    options.Cookie.HttpOnly = true;                 // Bảo mật
    options.Cookie.IsEssential = true;              // GDPR compliance
});

// ⚙️ Dependency Injection - Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// 📷 HttpContextAccessor (để dùng Session trong Services)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// =============================================
// 2. CONFIGURE THE HTTP REQUEST PIPELINE
// =============================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ QUAN TRỌNG: Session phải đứng trước Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 🛣️ Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();