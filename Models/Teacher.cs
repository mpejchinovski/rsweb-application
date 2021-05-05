using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
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

        public string FullName
        {
            get { return String.Format("{0} {1}", FirstName, LastName); }
        }

        public ICollection<Course> CoursesFirst { get; set; }
        public ICollection<Course> CoursesSecond { get; set; }
    }
}
