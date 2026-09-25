using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context
                = context;
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        public IActionResult Result() => View();
        public IActionResult PayFees() => View();
        public IActionResult ChangePassword() => View();
        public IActionResult UploadPassport() => View();
        public IActionResult CourseRegistration() => View();
        public async Task<IActionResult> Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail")
                        ?? HttpContext.Session.GetString("Email")
                        ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                return View(user);
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
            if (student != null)
            {
                var mappedUser = new User
                {
                    FullName = student.FirstName,
                    Email = student.Email,
                    Password = "temp123"
                };
                return View(mappedUser);
            }

            var fallbackName = HttpContext.Session.GetString("UserName") ?? "Student";
            return View(new User
            {
                FullName = fallbackName,
                Email = email,
                Password = "temp123"
            });
        }
    }
}
