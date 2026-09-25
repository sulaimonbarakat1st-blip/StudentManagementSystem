using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StudentManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres"))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':', 2);
                var username = userInfo[0];
                var password = userInfo.Length > 1? userInfo[1] : "";
                var database = uri.AbsolutePath.Trim('/').Split('?')[0];
                var port = uri.Port > 0? uri.Port : 5432;
                connectionString = $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SslMode=Require;Trust Server Certificate=true;";
            }

            builder.Services.AddDbContext<ApplicationDbContext>(options => {
                options.UseNpgsql(connectionString);
                options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            });

            var app = builder.Build();

            // ONLY auto-migrate on Render (Production). Locally, don't crash.
            if (!app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    db.Database.Migrate();
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            app.Run();
        }
    }
}