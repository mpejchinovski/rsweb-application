using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class EnrollmentFilterViewModel
    {
        public IEnumerable<Enrollment> Enrollments { get; set; }
        public int Year { get; set; }
    }
}
