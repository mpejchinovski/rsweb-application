using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly WebAppDbContext _context;

        public EnrollmentsController(WebAppDbContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index(int? courseId, int? studentId, int? year)
        {
            ViewData["Year"] = new SelectList(Enumerable.Range(2000, (DateTime.Now.Year - 2000) + 1)).Reverse();

            IQueryable<Enrollment> enrollments = _context.Enrollments.Include(e => e.Course).Include(e => e.Student).ThenInclude(s => s.User);

            if (courseId != null && studentId != null)
            {
                enrollments = enrollments.Where(e => e.CourseId == courseId && e.StudentId == studentId);
            } else
            {
                enrollments = courseId != null ? enrollments.Where(e => e.CourseId == courseId) : enrollments;
                enrollments = studentId != null ? enrollments.Where(e => e.StudentId == studentId) : enrollments;
            }

            if (User.IsInRole("Student"))
            {
                string userIdValue;
                if (User.Identity is ClaimsIdentity claimsIdentity)
                {
                    var userIdClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                    if (userIdClaim != null)
                    {
                        userIdValue = userIdClaim.Value;
                        Console.WriteLine(userIdValue);

                        if (studentId != null && studentId != await _context.Students.Where(s => s.UserId == userIdValue).Select(s => s.Id).FirstOrDefaultAsync())
                            return Redirect("Identity/Account/AccessDenied");

                        if (courseId != null)
                            return Redirect("Identity/Account/AccessDenied");

                        var teacherId = await _context.Teachers.Where(t => t.UserId == userIdValue).Select(t => t.Id).FirstOrDefaultAsync();
                        enrollments = enrollments.Where(e => e.Student.User.Id == userIdValue);
                    }
                }
            }

            enrollments = year != null ? enrollments.Where(e => e.Year == year) : enrollments;

            EnrollmentFilterViewModel enrollmentFilterViewModel = new EnrollmentFilterViewModel
            {
                Enrollments = await enrollments.ToListAsync()
            };

            return View(enrollmentFilterViewModel);
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create(int? courseId, int? studentId)
        {
            ViewData["CourseId"] = courseId != null ? new SelectList(_context.Courses.Where(c => c.Id == courseId), "Id", "Title") :
                new SelectList(_context.Courses, "Id", "Title");
            ViewData["StudentId"] = studentId != null ? new SelectList(_context.Students.Where(s => s.Id == studentId).Include(s => s.User), "Id", "User.FullName") :
                new SelectList(_context.Students.Include(s => s.User), "Id", "User.FullName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", enrollment.StudentId);

            var enrollmentsExists = _context.Enrollments
                .Any(e => e.StudentId == enrollment.StudentId && e.CourseId == enrollment.CourseId);

            if (enrollmentsExists)
            {
                ModelState.AddModelError("Enrollment", "Student already enrolled in course");
                return View(enrollment);
            }

            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses.Where(c => c.Id == enrollment.CourseId), "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students.Where(s => s.Id == enrollment.StudentId).Include(s => s.User), "Id", "User.FullName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(long id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}
