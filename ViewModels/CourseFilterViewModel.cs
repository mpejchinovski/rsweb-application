using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class CourseFilterViewModel
    {
        public IList<Course> Courses { get; set; }
        public string TitleString { get; set; }
        public string Semester { get; set; }
        public string ProgrammeString { get; set; }
    }
}
