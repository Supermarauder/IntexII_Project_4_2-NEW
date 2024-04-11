using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;

namespace IntexII_Project_4_2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var modelPath = "model_final.onnx";
            using var session = new InferenceSession(modelPath);

            foreach (var input in session.InputMetadata)
            {
                Console.WriteLine($"Input Name: {input.Key}");
                Console.WriteLine($"Input Type: {input.Value.ElementType}");
                Console.WriteLine($"Input Dimensions: {string.Join(",", input.Value.Dimensions)}");
            }

            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;

            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            });

            // Add services to the container.
            builder.WebHost.ConfigureKestrel(opt => opt.AddServerHeader = false);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add session services
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Or whatever suits your application
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddScoped<IIntexProjectRepository, EFIntexProjectRepository>();

            builder.Services.AddControllersWithViews();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 1;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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

            app.UseSession(); // Use session middleware

            app.UseAuthentication(); // Use authentication middleware (if needed, adjust accordingly)
            app.UseAuthorization();

            app.Use(async (context, next) => {

                context.Response.Headers.Add("X-Context-Type-Options", "nosniff");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; img-src 'self' data: m.media-amazon.com images.brickset.com www.brickeconomy.com *.amazonaws.com *.lego.com; script-src 'self' www.google.com app.termly.io; style-src 'self' 'unsafe-inline'; object-src 'none'");

                context.Response.Headers.Remove("X-Powered-By");
                context.Response.Headers.Remove("Server");

                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("productCategory", "{productCategory}", new { Controller = "Home", action = "ViewProducts", pageNum = 1 });

            app.MapDefaultControllerRoute();
            app.MapRazorPages();

            //Creates roles
            //using (var scope = app.Services.CreateScope())
            //{
            //    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //    var roles = new[] { "Admin", "Manager", "Member" };

            // //foreach (var role in roles)
            // //   {
            // //   if (!await roleManager.RoleExistsAsync(role))
            // //       await roleManager.CreateAsync(new IdentityRole(role));
            // //   }
            //}


            ////Creates Admin account
            //using (var scope = app.Services.CreateScope())
            //{
            //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            //    string email = "admin@admin.com";
            //    string password = "RootbeerWillNeverDie@2024";

            //    if(await userManager.FindByEmailAsync(email) == null)
            //    {
            //        var user = new IdentityUser();
            //        user.UserName = email;
            //        user.Email = email;
            //        user.EmailConfirmed = true;

            //        await userManager.CreateAsync(user, password);

            //        await userManager.AddToRoleAsync(user, "Admin");
            //    }
            //}


            app.Run();
        }
    }
}

