using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    [Index(nameof(UserId), IsUnique = true)]
    public class Student
    {
        public long Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [StringLength(10), Required]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; }

        [DataType(DataType.Date), Display(Name = "Enrollment date")]
        public DateTime? EnrollmentDate { get; set; }

        [Display(Name = "Acquired credits")]
        public int? AcquiredCredits { get; set; }

        [Display(Name = "Current semester")]
        public int? CurrentSemester { get; set; }

        [StringLength(25)]
        [Display(Name = "Education level")]
        public string EducationLevel { get; set; }

        public ICollection<Enrollment> EnrolledIn { get; set; }
    }
}
