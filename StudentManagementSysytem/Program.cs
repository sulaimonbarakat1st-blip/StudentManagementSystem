using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using ApplicationDbContext = StudentManagementSystem.Data.ApplicationDbContext;

namespace StudentManagementSystem
{
    public class 
        
        
        Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession();
            builder.Services.AddDbContext<StudentManagementSystem.Data.ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StudentManagementSystem.Data.ApplicationDbContext>();
                db.Database.Migrate();
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