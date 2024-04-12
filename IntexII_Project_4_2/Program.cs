using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;

namespace IntexII_Project_4_2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;

            //Initialize Key Vault Client
            var keyVaultUrl = builder.Configuration["KeyVault:VaultUri"]; // Ensure you have this in your appsettings or as an environment variable
            var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            //Fetch secrets from Azure Key Vault
            var googleClientId = secretClient.GetSecret("Google-Client-ID").Value.Value;
            var googleClientSecret = secretClient.GetSecret("Google-Client-Secret").Value.Value;

            //Deprecated connection of doing authentication
            //services.AddAuthentication().AddGoogle(googleOptions =>
            //{
            //    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
            //    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            //});

            // Add services to the container.
            builder.WebHost.ConfigureKestrel(opt => opt.AddServerHeader = false);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add SignInManager service
            builder.Services.AddScoped<SignInManager<ApplicationUser>>();

            // Add session services
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Or whatever suits your application
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddScoped<IIntexProjectRepository, EFIntexProjectRepository>();

            builder.Services.AddControllersWithViews();

            ////Configure Google Authentication with Key Vault secrets
            builder.Services.AddAuthentication().AddGoogle(googleOptions =>
            {
              googleOptions.ClientId = googleClientId;
              googleOptions.ClientSecret = googleClientSecret;
            });


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

            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365); // Adjust the MaxAge as needed
            });

            builder.Services.AddSingleton<InferenceSession>(serviceProvider =>
            {
                var env = serviceProvider.GetService<IHostEnvironment>();
                var modelPath = Path.Combine(env.ContentRootPath, "Final_Model.onnx");
                return new InferenceSession(modelPath);
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

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession(); // Use session middleware

            app.UseAuthentication(); // Use authentication middleware (if needed, adjust accordingly)
            app.UseAuthorization();

            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                ctx.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                ctx.Response.Headers.Add("Referrer-Policy", "no-referrer");
                ctx.Response.Headers.Add("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://app.termly.io; " + // Add https://app.termly.io here
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' https://images.brickset.com https://www.lego.com https://*.amazonaws.com https://*.googleusercontent.com https://m.media-amazon.com https://www.brickeconomy.com data:; " +
                    "font-src 'self'; " +
                    "frame-src 'self'; " +
                    "object-src 'none'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'; " +
                    "connect-src 'self';");
                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("productCategory", "{productCategory}", new { Controller = "Home", action = "ViewProducts", pageNum = 1 });

            app.MapDefaultControllerRoute();
            app.MapRazorPages();

            //Creates roles
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Admin", "Manager", "Member" };

             foreach (var role in roles)
                {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }


            ////Creates Admin account
            //using (var scope = app.Services.CreateScope())
            //{
            //    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //    string email = "admin@admin.com";
            //    string password = "RootbeerWillNeverDie@2024";

            ////    if(await userManager.FindByEmailAsync(email) == null)
            ////    {
            ////        var user = new ApplicationUser();
            ////        user.UserName = email;
            ////        user.Email = email;
            ////        user.EmailConfirmed = true;

            //        await userManager.CreateAsync(user, password);

            //        await userManager.AddToRoleAsync(user, "Admin");
            //    }
            //}


            app.Run();
        }
    }
}

