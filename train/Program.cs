using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data; // contains appdbcontext + appusercontext
using Microsoft.AspNetCore.Identity.UI.Services;
using train.Infrastructure;
using train.Repositories.Interface;
using train.Repositories;
using train.Repositories.Abstractions;
using train.Hubs;
using train.Services;


var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register DbContext
builder.Services.AddDbContext<appdbcontext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSignalR();

// Register Identity ONCE with your custom user + roles
builder.Services.AddIdentity<appusercontext, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // ⬅ allow login without email confirmation
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<appdbcontext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, ConsoleEmailSender>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IAdminStatsRepository, AdminStatsRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IStockAlertService, StockAlertService>();
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();



var app = builder.Build();

// ✅ Seed roles + admin
await IdentitySeeder.SeedAsync(app.Services);

// ✅ Seed sample data (categories + products)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<appdbcontext>();
    await train.Seed.DataSeeder.SeedDataAsync(dbContext);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (ctx, next) =>
{
    const string cookieName = "sf_sid";
    if (!ctx.Request.Cookies.TryGetValue(cookieName, out var _))
    {
        ctx.Response.Cookies.Append(cookieName, Guid.NewGuid().ToString("N"), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();


app.MapHub<StockHub>("/hubs/stock");
app.MapHub<AdminHub>("/hubs/admin");
app.MapHub<OrderHub>("/hubs/orders");
app.MapHub<CartHub>("/hubs/cart");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


app.Run();
