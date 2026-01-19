using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; // for IHostEnvironment
using System;
using System.Linq;
using System.Threading.Tasks;
using train.Areas.Identity.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<appdbcontext>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<appusercontext>>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        await ctx.Database.MigrateAsync();

        // 1) Roles
        foreach (var r in new[] { "Admin", "Customer" })
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));

        // 2) Admin user
        var adminEmail = "yasinabdullah5655@gmail.com";
        var adminPwd = "@bdu11@hY1"; // meets default policy

        var admin = await userMgr.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new appusercontext
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumber = "0000000000",
                City = "HQ",
                Age = 30
            };

            var create = await userMgr.CreateAsync(admin, adminPwd);
            if (!create.Succeeded)
            {
                var errors = string.Join("; ", create.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception($"Failed to create admin user: {errors}");
            }
        }
        else
        {
            // Ensure confirmed (important when RequireConfirmedAccount = true)
            if (!admin.EmailConfirmed)
            {
                var token = await userMgr.GenerateEmailConfirmationTokenAsync(admin);
                var ok = await userMgr.ConfirmEmailAsync(admin, token);
                if (!ok.Succeeded)
                {
                    var errors = string.Join("; ", ok.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception($"Failed to confirm admin email: {errors}");
                }
            }

            // In Development, force-reset password so you know it
            if (env.IsDevelopment())
            {
                var resetToken = await userMgr.GeneratePasswordResetTokenAsync(admin);
                var reset = await userMgr.ResetPasswordAsync(admin, resetToken, adminPwd);
                if (!reset.Succeeded)
                {
                    var errors = string.Join("; ", reset.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new Exception($"Failed to reset admin password: {errors}");
                }
            }
        }

        // 3) Ensure Admin role assignment
        if (!await userMgr.IsInRoleAsync(admin, "Admin"))
        {
            var addRole = await userMgr.AddToRoleAsync(admin, "Admin");
            if (!addRole.Succeeded)
            {
                var errors = string.Join("; ", addRole.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception($"Failed to add admin to Admin role: {errors}");
            }
        }
    }
}
