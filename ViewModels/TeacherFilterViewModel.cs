using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class TeacherFilterViewModel
    {
        public IList<Teacher> Teachers { get; set; }
        public string NameString { get; set; }
        public string DegreeString { get; set; }
        public string RankString { get; set; }
    }
}
