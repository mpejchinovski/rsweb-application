using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, WebAppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            ViewData["ProfilePicture"] = String.IsNullOrEmpty(student.User.ProfilePicture) ? "default.png" : student.User.ProfilePicture;

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

            ViewData["ProfilePicture"] = String.IsNullOrEmpty(student.User.ProfilePicture) ? "default.png" : student.User.ProfilePicture;

            StudentUserViewModel studentUserViewModel = new StudentUserViewModel
            {
                Student = student,
            };

            return View(studentUserViewModel);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentUserViewModel studentUserViewModel)
        {
            if (id != studentUserViewModel.Student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadedFile(studentUserViewModel);

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentUserViewModel.Student.UserId);
                    user.FirstName = studentUserViewModel.Student.User.FirstName;
                    user.LastName = studentUserViewModel.Student.User.LastName;
                    user.ProfilePicture = uniqueFileName;

                    studentUserViewModel.Student.User = null;

                    _context.Entry(user).State = EntityState.Modified;
                    _context.Update(studentUserViewModel.Student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(studentUserViewModel.Student.Id))
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
            return View(studentUserViewModel);
        }

        private string UploadedFile(StudentUserViewModel model)
        {
            string uniqueFileName = null;

            if (model.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfilePicture.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
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
