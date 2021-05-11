using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class StudentsController : Controller
    {
        private readonly WebAppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public StudentsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, WebAppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Students
        public IActionResult Index(string NameString, string IdString)
        {
            var students = _context.Students.AsQueryable();
            if (!String.IsNullOrEmpty(NameString))
            {
                students = students.Where(s => s.User.FirstName.Contains(NameString) || s.User.LastName.Contains(NameString));
            }
            if (!String.IsNullOrEmpty(IdString))
            {
                students = students.Where(s => s.StudentId.Contains(IdString));
            }

            StudentFilterViewModel studentFilterVM = new StudentFilterViewModel
            {
                Students = students.Include(s => s.User).ToList()
            };

            return View(studentFilterVM);
        }

        // GET: Students/5/Enrollments
        [Route("Students/{id?}/Enrollments")]
        public async Task<IActionResult> Enrollments(long? id)
        {
            var enrollments = await _context.Enrollments.Where(e => e.StudentId == id).Include(e => e.Course).ToListAsync();

            ViewData["StudentName"] = _context.Students.Find(id).User;
            return View(enrollments);
        }

        public IActionResult EnrollStudent(int id)
        {
            return RedirectToAction("Create", "Enrollments", new { studentId = id });
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(long? id)
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

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentUserViewModel studentUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FirstName = studentUserViewModel.Student.User.FirstName,
                    LastName = studentUserViewModel.Student.User.LastName,
                    UserName = new MailAddress(studentUserViewModel.Student.User.Email).User,
                    Email = studentUserViewModel.Student.User.Email
                };

                if (await _userManager.Users.AnyAsync(u => u.Email == studentUserViewModel.Student.User.Email))
                {
                    ModelState.AddModelError("Email", "User with that email already exists!");
                    return View(studentUserViewModel);
                }

                studentUserViewModel.Student.User = null;

                var result = await _userManager.CreateAsync(user, studentUserViewModel.Password);

                if (result.Succeeded)
                {
                    studentUserViewModel.Student.UserId = user.Id;
                    _context.Add(studentUserViewModel.Student);
                    await _userManager.AddToRoleAsync(user, "Student");
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                };
            }
            return View(studentUserViewModel);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(long? id)
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

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Students.Where(s => s.Id == id).Include(s => s.User).FirstOrDefaultAsync();

            _context.Students.Remove(student);
            await _userManager.DeleteAsync(student.User);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
