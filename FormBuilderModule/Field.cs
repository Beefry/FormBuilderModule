using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beefry.FormBuilder
{
    public class Field
    {
        public int? ID { get; set; }
        public int? FormID { get; set; }
        public string Label { get; set; }
        public bool Required { get; set; }
        public int SortOrder { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public List<Option> Options { get; set; }

        public Field()
        {
            this.Options = new List<Option>();
        }
    }
}
