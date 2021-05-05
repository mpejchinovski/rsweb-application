using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.ViewModels
{
    public class StudentFilterViewModel
    {
        public IList<Student> Students { get; set; }
        public string NameString { get; set; }
        public string IdString { get; set; }
    }
}
