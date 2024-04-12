using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Threading.Tasks;

namespace IntexII_Project_4_2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;

            // Initialize Key Vault Client
            var keyVaultUrl = configuration["KeyVault:VaultUri"];
            var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            // Fetch secrets from Azure Key Vault
            var googleClientId = secretClient.GetSecret("Google-Client-ID").Value.Value;
            var googleClientSecret = secretClient.GetSecret("Google-Client-Secret").Value.Value;


            // Database connection
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Identity configuration
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddScoped<SignInManager<ApplicationUser>>();

            // Session configuration
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookies are secure
                options.Cookie.SameSite = SameSiteMode.None; // Important for OAuth redirects
            });

            // Repository for custom project operations
            services.AddScoped<IIntexProjectRepository, EFIntexProjectRepository>();

            // MVC controllers and views
            services.AddControllersWithViews();

            // Configure Google Authentication with Key Vault secrets
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = googleClientId;
                googleOptions.ClientSecret = googleClientSecret;
                googleOptions.CallbackPath = new PathString("/signin-google");
                googleOptions.Events = new OAuthEvents
                {
                    OnRemoteFailure = context =>
                    {
                        // Handle remote login failures here
                        context.Response.Redirect("/login-error");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });

            // HSTS configuration
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            // ML Model configuration
            services.AddSingleton<InferenceSession>(serviceProvider =>
            {
                var env = serviceProvider.GetService<IHostEnvironment>();
                var modelPath = Path.Combine(env.ContentRootPath, "Final_Model.onnx");
                return new InferenceSession(modelPath);
            });

            var app = builder.Build();

            // HTTP request pipeline configuration
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapDefaultControllerRoute();
            app.MapRazorPages();

            // Role creation
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Admin", "Manager", "Member" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }

            app.Run();
        }
    }
}
