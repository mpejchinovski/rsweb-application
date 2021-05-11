using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    [Index(nameof(UserId), IsUnique = true)]
    public class Teacher
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [StringLength(50)]
        [Display(Name = "Degree")]
        public string Degree { get; set; }

        [StringLength(25)]
        [Display(Name = "Academic rank")]
        public string AcademicRank { get; set; }

        [StringLength(10)]
        [Display(Name = "Office number")]
        public string OfficeNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hire date")]
        public DateTime? HireDate { get; set; }

        public ICollection<Course> CoursesFirst { get; set; }
        public ICollection<Course> CoursesSecond { get; set; }
    }
}
