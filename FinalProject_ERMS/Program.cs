using FinalProject_ERMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinalProject_ERMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();


            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>() 
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Home/AccessDenied";
            });


            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
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
            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                    // Define roles
                    string[] roles = { "Admin", "Manager", "Employee" };

                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                        }
                    }

                    // Create admin user
                    var adminEmail = "admin@company.com";
                    var adminPassword = "Admin123!"; 

                    if (await userManager.FindByEmailAsync(adminEmail) == null)
                    {
                        var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                        var result = await userManager.CreateAsync(adminUser, adminPassword);

                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding roles and admin user: {ex.Message}");
                }
            }

            await app.RunAsync(); 
        }
    }
}
