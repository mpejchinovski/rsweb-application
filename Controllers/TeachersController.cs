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
    public class TeachersController : Controller
    {
        private readonly WebAppDbContext _context;

        public TeachersController(WebAppDbContext context)
        {
            _context = context;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(string NameString, string DegreeString, string RankString)
        {
            IQueryable<Teacher> teachers = _context.Teachers.AsQueryable();

            if (!String.IsNullOrEmpty(NameString))
            {
                teachers = teachers.Where(c => c.FirstName.Contains(NameString) || c.LastName.Contains(NameString));
            }
            if (!String.IsNullOrEmpty(DegreeString))
            {
                teachers = teachers.Where(c => c.Degree.Contains(DegreeString));
            }
            if (!String.IsNullOrEmpty(RankString))
            {
                teachers = teachers.Where(c => c.AcademicRank.Contains(RankString));
            }

            TeacherFilterViewModel teacherFilterVM = new TeacherFilterViewModel
            {
                Teachers = await teachers.ToListAsync()
            };

            return View(teacherFilterVM);
        }

        // GET: Teachers/5/Courses
        [Route("Teachers/{id?}/Courses")]
        public ActionResult Courses (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var courses = _context.Courses.Where(c => c.FirstTeacherId == id || c.SecondTeacherId == id);
            var teacher = _context.Teachers.Where(t => t.Id == id);

            TeacherCoursesViewModel TeacherCoursesVM = new TeacherCoursesViewModel
            {
                Teacher = teacher.FirstOrDefault(),
                Courses = courses.ToList()
            };

            return View(TeacherCoursesVM);
        }

        // GET: Teachers/5/Courses/5/Leave
        [HttpGet]
        [Route("Teachers/{id}/Courses/{courseId}/Leave")]
        public async Task<IActionResult> Leave([FromRoute] int courseId)
        {
            var course = await _context.Courses.Where(c => c.Id == courseId).Include(c => c.Teacher1).Include(c => c.Teacher2).FirstAsync();
            if (course == null)
            {
                return NotFound();
            }

            return View("~/Views/Teachers/CourseLeave.cshtml", course);
        }

        // POST: Teachers/5/Courses/5/Leave
        [HttpPost]
        [Route("Teachers/{id}/Courses/{courseId}/Leave")]
        public async Task<IActionResult> Leave([FromRoute] int id, [FromRoute] int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

            if (course.FirstTeacherId != null && course.FirstTeacherId == id)
            {
                course.FirstTeacherId = null;
            } else
            {
                course.SecondTeacherId = null;
            }

            _context.Entry(course).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction("Courses", new { id });
        }

        // GET: Teachers/5/Courses/5/Details
        [Route("Teachers/{id?}/Courses/{courseId}/Details")]
        public async Task<IActionResult> CourseDetails([FromRoute]int? id, [FromRoute]int courseId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Teacher1)
                .Include(c => c.Teacher2)
                .FirstOrDefaultAsync(m => m.Id == courseId);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Teachers/5/Details
        [Route("Teachers/{id?}/Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
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
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
