using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;
using ClosedXML.Excel;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Globalization;
using System.Text.Json;

namespace StudentManagementSystem.Controllers
{

    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchString, string facultyFilter)
        {
            // Start with all students + include related tables
            var students = _context.Students
    .AsQueryable();

            // 1. CARD DATA
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.FacultyCount = await _context.Faculties.CountAsync(); // if you have Faculties table
            ViewBag.NewThisMonth = await _context.Students
                .Where(s => s.DateOfBirth.Month == DateTime.Now.Month && s.DateOfBirth.Year == DateTime.Now.Year)
                .CountAsync();

            // 2. SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.FirstName.Contains(searchString)
                                            || s.LastName.Contains(searchString)
                                            || s.Email.Contains(searchString));
            }

            // 3. FILTER BY FACULTY
            if (!string.IsNullOrEmpty(facultyFilter))
            {
                students = students.Where(s => s.Faculty == facultyFilter);
            }

            // 4. DROPDOWN: Get distinct faculty list
            var faculties = await _context.Students
                .Select(s => s.Faculty)
                .Distinct()
                .ToListAsync();
            ViewData["Faculties"] = new SelectList(faculties);
            ViewData["CurrentFilter"] = searchString;

            // 5. BAR CHART DATA: Students per Faculty
            var chartData = await _context.Students
                .GroupBy(s => s.Faculty)
                .Select(g => new { Faculty = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.ChartLabels = JsonSerializer.Serialize(chartData.Select(x => x.Faculty));
            ViewBag.ChartData = JsonSerializer.Serialize(chartData.Select(x => x.Count));

            return View(await students.ToListAsync());
        }

            
        // GET: Student/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

// GET: Student/ExportToExcel
public IActionResult ExportToExcel()
    {
        var students = _context.Students.ToList();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Students");
            var currentRow = 1;

            // Headers
            worksheet.Cell(currentRow, 1).Value = "Student ID";
            worksheet.Cell(currentRow, 2).Value = "First Name";
            worksheet.Cell(currentRow, 2).Value = "MatricNo";
            worksheet.Cell(currentRow, 3).Value = "Last Name";
            worksheet.Cell(currentRow, 4).Value = "Email";
            worksheet.Cell(currentRow, 5).Value = "Faculty";
            worksheet.Cell(currentRow, 6).Value = "Department";
            worksheet.Cell(currentRow, 7).Value = "Date of Birth";

            // Data
            foreach (var student in students)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = "STU-" + student.Id.ToString("D4");
                worksheet.Cell(currentRow, 2).Value = student.FirstName;
                worksheet.Cell(currentRow, 3).Value = student.LastName;
                worksheet.Cell(currentRow, 4).Value = student.Email;
                worksheet.Cell(currentRow, 5).Value = student.Faculty;
                worksheet.Cell(currentRow, 6).Value = student.Department;
                worksheet.Cell(currentRow, 7).Value = student.DateOfBirth.ToString("dd-MMM-yyyy");
            }
                worksheet.Columns().Width = 20;
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentsList.xlsx");
            }
        }
    }

        // GET: Student/ExportToPdf
        public IActionResult ExportToPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var students = _context.Students.ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Student List Report").FontSize(20).Bold().FontColor(Colors.Blue.Medium);

                    page.Content().Table(table =>
                    {
                        // Define 7 columns to match your Excel
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // ID
                            columns.RelativeColumn(2); // First
                            columns.RelativeColumn(2); // Last
                            columns.RelativeColumn(3); // Email
                            columns.RelativeColumn(2); // Faculty
                            columns.RelativeColumn(2); // Department
                            columns.RelativeColumn(2); // DOB
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Student ID").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("First Name").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Last Name").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Email").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Faculty").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Department").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("DOB").Bold();
                        });

                        // Data rows - matches your Excel
                        foreach (var student in students)
                        {
                            table.Cell().Padding(5).Text("STU-" + student.Id.ToString("D4"));
                            table.Cell().Padding(5).Text(student.FirstName);
                            table.Cell().Padding(5).Text(student.LastName);
                            table.Cell().Padding(5).Text(student.Email);
                            table.Cell().Padding(5).Text(student.Faculty);
                            table.Cell().Padding(5).Text(student.Department);
                            table.Cell().Padding(5).Text(student.DateOfBirth.ToString("dd-MMM-yyyy"));
                        }
                    });
                    page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "Students.pdf");
        }
        public IActionResult Create()
        {
            return View();
        }
        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Student student)
        {
            // 1. CHECK FOR DUPLICATE EMAIL - ADD THIS
            var emailExists = await _context.Students.AnyAsync(s =>
  s.Email.ToLower() == student.Email.ToLower());
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This Email already exists. Please use a different one.");
            }

            if (ModelState.IsValid)
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                TempData["Success!"] = "Student Registered Successfully";
                return RedirectToAction(nameof(Index));
            }

            // This line will now also show the "Email already exists" error
            return View(student);
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // POST: Student/Edit/5
      

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }
        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id) return NotFound();

            // 1. CHECK FOR DUPLICATE EMAIL - ADD THIS

            var emailExists = await _context.Students.AnyAsync(s => s.Email.Equals(student.Email, StringComparison.OrdinalIgnoreCase));
            {
                ModelState.AddModelError("Email", "This Email already exists. Please use a different one.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null) _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}