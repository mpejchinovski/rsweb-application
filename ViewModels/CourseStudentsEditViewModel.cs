using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class CourseStudentsEditViewModel
    {
        public Course Course { get; set; }
        public IEnumerable<int> SelectedStudents { get; set; }
        public IEnumerable<SelectListItem> StudentList { get; set; }
    }
}
