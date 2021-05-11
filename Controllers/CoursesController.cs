using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Index(string TitleString, int Semester, string ProgrammeString, int? teacherId)
        {
            IQueryable<Course> courses = _context.Courses.Include(c => c.Teacher1).ThenInclude(t1 => t1.User).Include(c => c.Teacher2).ThenInclude(t2 => t2.User);

            courses = teacherId != null ? courses.Where(c => c.Teacher1.Id == teacherId || c.Teacher2.Id == teacherId) : courses;
            ViewBag.Title = teacherId != null ? await _context.Teachers.Where(t => t.Id == teacherId).Select(t => t.User.FullName).FirstOrDefaultAsync() + "'s courses" : "Index";

            if (User.IsInRole("Teacher"))
            {
                string userIdValue;
                if (User.Identity is ClaimsIdentity claimsIdentity)
                {
                    var userIdClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                    if (userIdClaim != null)
                    {
                        userIdValue = userIdClaim.Value;
                        Console.WriteLine(userIdValue);

                        if (teacherId != null && teacherId != await _context.Teachers.Where(t => t.UserId == userIdValue).Select(t => t.Id).FirstOrDefaultAsync())
                            return Redirect("Identity/Account/AccessDenied");

                        var currentTeacherId = await _context.Teachers.Where(t => t.UserId == userIdValue).Select(t => t.Id).FirstOrDefaultAsync();

                        ViewData["CourseId"] = new SelectList(_context.Courses.Where(c => c.FirstTeacherId == currentTeacherId || c.SecondTeacherId == currentTeacherId), "Id", "Title");
                        ViewData["Year"] = new SelectList(Enumerable.Range(2000, (DateTime.Now.Year - 2000) + 1)).Reverse();

                        return View("~/Views/Courses/StudentList.cshtml");
                    }
                }
            }

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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> teachers =
                from teacher in _context.Teachers.Include(t => t.User)
                select new SelectListItem
                {
                    Selected = false,
                    Text = teacher.User.FullName,
                    Value = String.Format("{0}", teacher.Id)
                };

            ViewData["FirstTeacherId"] = teachers;
            ViewData["SecondTeacherId"] = teachers;
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers.Include(t => t.User), "Id", "FullName");
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers.Include(t => t.User), "Id", "FullName");
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

            CourseStudentsEditViewModel viewmodel = new CourseStudentsEditViewModel
            {
                Course = course,
                SelectedStudents = _context.Enrollments.Where(e => e.CourseId == id).Select(e => e.Student).Select(s => (int)s.Id),
                StudentList = new MultiSelectList(_context.Students.Include(s => s.User).AsEnumerable(), "Id", "User.FullName")
            };

            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers.Include(t => t.User), "Id", "User.FullName");
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers.Include(t => t.User), "Id", "User.FullName");
            return View(viewmodel);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseStudentsEditViewModel viewmodel)
        {
            if (id != viewmodel.Course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewmodel.Course);
                    await _context.SaveChangesAsync();

                    IEnumerable<int> listStudents = viewmodel.SelectedStudents;
                    if (listStudents != null && listStudents.Any())
                    {
                        IQueryable<Enrollment> toBeRemoved = _context.Enrollments.Where(s => !listStudents.Contains((int)s.StudentId) && s.CourseId == id);

                        _context.Enrollments.RemoveRange(toBeRemoved.AsEnumerable());

                        IEnumerable<int> existStudents = _context.Enrollments.Where(s => listStudents.Contains((int)s.StudentId) && s.CourseId == id).Select(s => (int)s.StudentId);
                        IEnumerable<int> newStudents = listStudents.Where(s => !existStudents.Contains(s));
                        foreach (int studentId in newStudents)
                            _context.Enrollments.Add(new Enrollment { StudentId = studentId, CourseId = id });

                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(viewmodel.Course.Id))
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

            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", viewmodel.Course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "Id", "FirstName", viewmodel.Course.SecondTeacherId);
            return View(viewmodel);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
