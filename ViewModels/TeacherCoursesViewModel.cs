using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class TeacherCoursesViewModel
    {
        public Teacher Teacher { get; set; }
        public IEnumerable<Course> Courses { get; set; }
    }
}
