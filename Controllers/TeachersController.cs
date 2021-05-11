using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class TeachersController : Controller
    {
        private readonly WebAppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TeachersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, WebAppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(string nameString, string degreeString, string rankString)
        {
            string userIdValue;

            if (User.Identity is ClaimsIdentity claimsIdentity)
            {
                var userIdClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    userIdValue = userIdClaim.Value;
                    Console.WriteLine(userIdValue);
                }
            }

            IQueryable<Teacher> teachers = _context.Teachers.Include(t => t.User);

            if (!String.IsNullOrEmpty(nameString))
            {
                teachers = teachers.Where(t => t.User.FirstName.Contains(nameString) || t.User.LastName.Contains(nameString));
            }
            if (!String.IsNullOrEmpty(degreeString))
            {
                teachers = teachers.Where(t => t.Degree.Contains(degreeString));
            }
            if (!String.IsNullOrEmpty(rankString))
            {
                teachers = teachers.Where(t => t.AcademicRank.Contains(rankString));
            }

            TeacherFilterViewModel teacherFilterVM = new TeacherFilterViewModel
            {
                Teachers = await teachers.Include(t => t.User).ToListAsync()
            };

            return View(teacherFilterVM);
        }

        // GET: Teachers/5/Details
        [Route("Teachers/{id?}/Details")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.Where(t => t.Id == id).Include(t => t.User).FirstOrDefaultAsync();

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
        public async Task<IActionResult> Create(TeacherUserViewModel teacherUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FirstName = teacherUserViewModel.Teacher.User.FirstName,
                    LastName = teacherUserViewModel.Teacher.User.LastName,
                    UserName = new MailAddress(teacherUserViewModel.Teacher.User.Email).User,
                    Email = teacherUserViewModel.Teacher.User.Email
                };

                if (await _userManager.Users.AnyAsync(u => u.Email == teacherUserViewModel.Teacher.User.Email))
                {
                    ModelState.AddModelError("Email", "User with that email already exists!");
                    return View(teacherUserViewModel);
                }

                teacherUserViewModel.Teacher.User = null;

                var result = await _userManager.CreateAsync(user, teacherUserViewModel.Password);

                if (result.Succeeded)
                {
                    teacherUserViewModel.Teacher.UserId = user.Id;
                    _context.Teachers.Add(teacherUserViewModel.Teacher);
                    await _userManager.AddToRoleAsync(user, "Teacher");
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(teacherUserViewModel);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.Where(t => t.Id == id).Include(t => t.User).FirstOrDefaultAsync();

            if (teacher == null)
            {
                return NotFound();
            }

            TeacherUserViewModel teacherUserViewModel = new TeacherUserViewModel
            {
                Teacher = teacher,
            };

            return View(teacherUserViewModel);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherUserViewModel teacherUserViewModel)
        {
            if (id != teacherUserViewModel.Teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacherUserViewModel.Teacher);
                    await _context.SaveChangesAsync();

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == teacherUserViewModel.Teacher.UserId);
                    user.FirstName = teacherUserViewModel.Teacher.User.FirstName;
                    user.LastName = teacherUserViewModel.Teacher.User.LastName;

                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacherUserViewModel.Teacher.Id))
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
            return View(teacherUserViewModel);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.Where(t => t.Id == id).Include(t => t.User).FirstOrDefaultAsync();

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
            var teacher = await _context.Teachers.Where(t => t.Id == id).Include(t => t.User).FirstOrDefaultAsync();

            _context.Teachers.Remove(teacher);
            await _userManager.DeleteAsync(teacher.User);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
