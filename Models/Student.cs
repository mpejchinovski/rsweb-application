using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class Student
    {
        public long Id { get; set; }
        [StringLength(10)]
        [Required]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; }
        [StringLength(50)]
        [Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [StringLength(50)]
        [Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Enrollment date")]
        public DateTime? EnrollmentDate { get; set; }
        [Display(Name = "Acquired credits")]
        public int? AcquiredCredits { get; set; }
        [Display(Name = "Current semester")]
        public int? CurrentSemester { get; set; }
        [StringLength(25)]
        [Display(Name = "Education level")]
        public string EducationLevel { get; set; }

        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }

        public ICollection<Enrollment> EnrolledIn { get; set; }
    }
}
