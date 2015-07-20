using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beefry.FormBuilder
{
    public class Section
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public string SortOrder { get; set; }
        public List<Field> Fields { get; set; }
    }
}
