using Auth0.AspNetCore.Authentication;

namespace SimpleWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services
                .AddAuth0WebAppAuthentication(options => {
                    options.Domain = builder.Configuration["Auth0:Domain"];
                    options.ClientId = builder.Configuration["Auth0:ClientId"];
                    options.Scope = "openid profile email";
                });

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Protected");
                options.Conventions.AuthorizePage("/Account/Logout");
                options.Conventions.AuthorizePage("/Account/Profile");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            // Add Auth0
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}