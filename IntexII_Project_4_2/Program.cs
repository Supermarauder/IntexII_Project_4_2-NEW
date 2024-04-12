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

            //// Initialize Key Vault Client
            //var keyVaultUrl = configuration["KeyVault:VaultUri"];
            //var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            //// Fetch secrets from Azure Key Vault
            //var googleClientId = secretClient.GetSecret("Google-Client-ID").Value.Value;
            //var googleClientSecret = secretClient.GetSecret("Google-Client-Secret").Value.Value;


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
            //services.AddAuthentication().AddGoogle(googleOptions =>
            //{
            //    googleOptions.ClientId = googleClientId;
            //    googleOptions.ClientSecret = googleClientSecret;
            //    googleOptions.CallbackPath = new PathString("/signin-google");
            //    googleOptions.Events = new OAuthEvents
            //    {
            //        OnRemoteFailure = context =>
            //        {
            //            // Handle remote login failures here
            //            context.Response.Redirect("/login-error");
            //            context.HandleResponse();
            //            return Task.CompletedTask;
            //        }
            //    };
            //});

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
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                ctx.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                ctx.Response.Headers.Add("Referrer-Policy", "no-referrer");

                // Updated Content-Security-Policy
                var csp = "default-src 'self'; " +
                          "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://app.termly.io https://apis.google.com https://accounts.google.com; " + // Allow scripts from Google APIs and Google Accounts
                          "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " + // Allow styles from Google Fonts
                          "img-src 'self' https://images.brickset.com https://www.lego.com https://*.amazonaws.com https://*.googleusercontent.com https://m.media-amazon.com https://www.brickeconomy.com data:; " + // Specify sources for images
                          "font-src 'self' https://fonts.gstatic.com; " + // Allow fonts from Google Fonts
                          "frame-src 'self' https://accounts.google.com; " + // Allow iframes from Google Accounts for OAuth
                          "object-src 'none'; " + // Disallow all objects
                          "base-uri 'self'; " +
                          "form-action 'self' https://accounts.google.com; " + // Allow form actions to self and Google Accounts
                          "connect-src 'self' https://accounts.google.com; " + // Allow connections to Google Accounts
                          "frame-ancestors 'self';"; // Restrict who can frame the page

                // Check if the current request is for the Google OAuth callback
                if (ctx.Request.Path.StartsWithSegments(new PathString("/signin-google")))
                {
                    // Modify the CSP to allow all sources for the OAuth callback
                    csp = "default-src * 'unsafe-inline' 'unsafe-eval';" +
                          "script-src * 'unsafe-inline' 'unsafe-eval';" +
                          "style-src * 'unsafe-inline';" +
                          "img-src * data:;" +
                          "font-src *;" +
                          "frame-src *;" +
                          "object-src *;" +
                          "base-uri 'self';" +
                          "form-action *;" +
                          "connect-src *;" +
                          "frame-ancestors *;";
                }

                ctx.Response.Headers.Add("Content-Security-Policy", csp);
                await next();
            });

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
