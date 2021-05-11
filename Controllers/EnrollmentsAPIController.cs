using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    public class EnrollmentsAPIController : ControllerBase
    {
        private readonly WebAppDbContext _context;

        public EnrollmentsAPIController(WebAppDbContext context)
        {
            _context = context;
        }

        public class JsonEnrollment
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Semester { get; set; }
            public int? Year { get; set; }
            public int? Grade { get; set; }
            public DateTime? FinishDate { get; set; }

            public JsonEnrollment(int id, string FirstName, string LastName, string Semester, int? Year, int? Grade, DateTime? FinishDate)
            {
                this.Id = id;
                this.FirstName = FirstName;
                this.LastName = LastName;
                this.Semester = Semester;
                this.Year = Year;
                this.Grade = Grade;
                this.FinishDate = FinishDate;
            }
        }

        // GET: api/EnrollmentsAPI
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JsonEnrollment>>> GetEnrollments(int? courseId, int? year)
        {
            var enrollments = _context.Enrollments.Include(e => e.Student).ThenInclude(s => s.User).AsQueryable();
            enrollments = year != null ? enrollments.Where(e => e.Year == year) : enrollments;

            IEnumerable<JsonEnrollment> results = courseId == null ? null :
                from e in await enrollments.Where(e => e.CourseId == courseId).ToListAsync()
                select new JsonEnrollment((int) e.Id, e.Student.User.FirstName, e.Student.User.LastName, e.Semester, e.Year, e.Grade, e.FinishDate);

            return Ok(results);
        }

        // GET: api/EnrollmentsAPI/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrollment>> GetEnrollment(long id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return enrollment;
        }

        // PUT: api/EnrollmentsAPI/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnrollment(long id, Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return BadRequest();
            }

            _context.Entry(enrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/EnrollmentsAPI
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Enrollment>> PostEnrollment(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEnrollment", new { id = enrollment.Id }, enrollment);
        }

        // DELETE: api/EnrollmentsAPI/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Enrollment>> DeleteEnrollment(long id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return enrollment;
        }

        private bool EnrollmentExists(long id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
    }
}
