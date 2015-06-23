using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beefry.FormBuilder
{
    public class Option
    {
        public int? ID { get; set; }
        public int FieldID { get; set; }
        public string Value { get; set; }
        public string SortOrder { get; set; }
    }
}
