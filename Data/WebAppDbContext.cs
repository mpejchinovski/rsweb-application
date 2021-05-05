using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApp.Data
{
    public class WebAppDbContext : IdentityDbContext<ApplicationUser>
    {
        public WebAppDbContext(DbContextOptions<WebAppDbContext> options) : base(options)
        { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Enrollment>()
                .HasOne<Student>(e => e.Student)
                .WithMany(s => s.EnrolledIn)
                .HasForeignKey(e => e.StudentId);

            builder.Entity<Enrollment>()
                .HasOne<Course>(e => e.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(e => e.CourseId);

            builder.Entity<Course>()
                .HasOne<Teacher>(c => c.Teacher1)
                .WithMany(t => t.CoursesFirst)
                .HasForeignKey(c => c.FirstTeacherId);

            builder.Entity<Course>()
                .HasOne<Teacher>(c => c.Teacher2)
                .WithMany(t => t.CoursesSecond)
                .HasForeignKey(c => c.SecondTeacherId);
        }
    }
}
