using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Email already exists");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // Now create his own Student row
                var count = _context.Students.Count() + 1;

                var names = model.FullName.Trim().Split(' ');
                var firstName = names[0];
                var lastName = names.Length > 1 ? names[names.Length - 1] : firstName;

                var student = new Student
                {
                    UserId = user.Id.ToString(),
                    MatricNo = $"CSC/2025/{count:D4}",
                    FirstName = firstName,
                    LastName = lastName,
                    Email = model.Email,
                    Department = "Computer Science",
                    Faculty = "Science",
                    DateOfBirth = DateTime.UtcNow
                };

                _context.Students.Add(student);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            // Add this to see why it stays
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(model);
        }
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower().Trim());
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }
                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Register", "Account");
        }
    }
}