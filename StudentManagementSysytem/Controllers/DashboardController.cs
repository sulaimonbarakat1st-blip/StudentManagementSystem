using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Data;

namespace StudentManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");
            var student = _context.Students.FirstOrDefault(s => s.UserId == userId);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.MatricNo = student?.MatricNo;
            ViewBag.FullName = student?.FirstName + " " + student?.LastName;

            return View();
        }


        public IActionResult Result()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");
            var student = _context.Students.FirstOrDefault(s => s.UserId == userId);

            if (student == null)
                return RedirectToAction("Login", "Account");

            // This makes every MatricNo get different but consistent result
            var seed = student.MatricNo.GetHashCode();
            var rnd = new Random(seed);

            ViewBag.MatricNo = student.MatricNo;
            ViewBag.FullName = student.FirstName + " " + student.LastName;

            // Different GPA for each student
            ViewBag.GPA = (2.5 + rnd.NextDouble() * 2.0).ToString("F2"); // 2.50 - 4.50
            ViewBag.CGPA = (2.8 + rnd.NextDouble() * 1.7).ToString("F2");
            ViewBag.Score1 = rnd.Next(55, 85); // CSC 101
            ViewBag.Score2 = rnd.Next(50, 80); // CSC 102
            ViewBag.Score3 = rnd.Next(45, 78); // MTH 101
            ViewBag.TotalUnits = 16;

            return View();
        }
        public IActionResult PayFees()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");
            var student = _context.Students.FirstOrDefault(s => s.UserId == userId);

            if (student == null)
                return RedirectToAction("Login", "Account");

            var rnd = new Random(student.MatricNo.GetHashCode() + 1);

            ViewBag.MatricNo = student.MatricNo;
            ViewBag.FullName = student.FirstName + " " + student.LastName;

            // Different for every student
            ViewBag.ReceiptNo = $"UNIOSUN/{DateTime.Now.Year}/{rnd.Next(1000, 9999)}";
            ViewBag.Amount = rnd.Next(120, 250) * 1000;
            ViewBag.AmountPaid = rnd.Next(0, 2) == 0 ? 0 : ViewBag.Amount;
            ViewBag.Status = ViewBag.AmountPaid == 0 ? "UNPAID" : "PAID";
            ViewBag.Balance = ViewBag.Amount - ViewBag.AmountPaid;

            return View();
        }

        public IActionResult Profile()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");
            var student = _context.Students.FirstOrDefault(s => s.UserId == userId);
            // Pass the real student object, not static text
            return View(student);
        }

        public IActionResult CourseRegistration()
        {
            if (HttpContext.Session.GetString("UserEmail") == null)
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");
            var student = _context.Students.FirstOrDefault(s => s.UserId == userId);
            var rnd = new Random(student.MatricNo.GetHashCode() + 2);

            ViewBag.MatricNo = student.MatricNo;
            ViewBag.FullName = student.FirstName + " " + student.LastName;
            ViewBag.Session = "2024/2025";
            ViewBag.TotalUnits = rnd.Next(15, 24); // 15-23 units
            ViewBag.RegStatus = rnd.Next(0, 2) == 0 ? "PENDING" : "APPROVED";

            return View();
        }

        public IActionResult UploadPassport()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var student = _context.Students.FirstOrDefault(s => s.UserId == userId);
            ViewBag.MatricNo = student?.MatricNo;
            ViewBag.FullName = student?.FirstName + " " + student?.LastName;
            return View();
        }
    }
}