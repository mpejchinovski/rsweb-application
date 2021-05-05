using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class CoursesController : Controller
    {
        private readonly WebAppDbContext _context;

        public CoursesController(WebAppDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string TitleString, int Semester, string ProgrammeString)
        {
            IQueryable<Course> courses = _context.Courses.AsQueryable();

            if (!String.IsNullOrEmpty(TitleString))
            {
                courses = courses.Where(c => c.Title.Contains(TitleString));
            }
            if (Semester != 0)
            {
                courses = courses.Where(c => c.Semester == Semester);
            }
            if (!String.IsNullOrEmpty(ProgrammeString))
            {
                courses = courses.Where(c => c.Programme == ProgrammeString);
            }

            courses = courses.Include(c => c.Teacher1).Include(c => c.Teacher2);

            CourseFilterViewModel CourseFilterVM = new CourseFilterViewModel
            {
                Courses = await courses.ToListAsync()
            };

            return View(CourseFilterVM);

        }

        public IActionResult EnrollStudent(int courseId)
        {
            return RedirectToAction("Create", "Enrollments", new { courseId });
        }

        // GET: Courses/5/Students
        [Route("Courses/{id?}/Enrollments")]
        public async Task<IActionResult> Enrollments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.Where(c => c.Id == id).FirstAsync();

            var enrollments = await _context.Enrollments.Where(e => e.CourseId == id)
                .Include(e => e.Student).Include(e => e.Course).ToListAsync();
            if (enrollments == null)
            {
                return NotFound();
            }

            ViewData["CourseTitle"] = course.Title;
            ViewData["CourseId"] = course.Id;
            return View(enrollments);
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher1)
                .Include(c => c.Teacher2)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName");
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {

            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.SecondTeacherId);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> EditAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var students = _context.Students.AsEnumerable();
            students = students.OrderBy(s => s.FullName);

            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FullName", course.SecondTeacherId);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
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
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", course.SecondTeacherId);
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher1)
                .Include(c => c.Teacher2)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
