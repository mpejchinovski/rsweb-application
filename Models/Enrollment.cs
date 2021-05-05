using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Models
{
    public class Enrollment
    {
        public long Id { get; set; }
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; }
        [Display(Name = "Student")]
        public long StudentId { get; set; }
        public Student Student { get; set; }
        [StringLength(10)]
        public string Semester { get; set; }
        public int? Year { get; set; }
        public int? Grade { get; set; }
        [StringLength(255)]
        [Display(Name = "Seminal URL")]
        public string SeminalUrl { get; set; }
        [StringLength(255)]
        [Display(Name = "Project URL")]
        public string ProjectUrl { get; set; }
        [Display(Name = "Exam points")]
        public int? ExamPoints { get; set; }
        [Display(Name = "Seminal points")]
        public int? SeminalPoints { get; set; }
        [Display(Name = "Project points")]
        public int? ProjectPoints { get; set; }
        [Display(Name = "Additional points")]
        public int? AdditionalPoints { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Finish date")]
        public DateTime? FinishDate { get; set; }
    }
}
