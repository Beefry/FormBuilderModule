using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

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
        public Value Value { get; set; }
        public List<Option> Options { get; set; }

        public Field()
        {
            this.Options = new List<Option>();
        }

        public bool Validate(DataRow fieldData) 
        {
            bool BoolTest = false;
            int IntTest = 0;

            if (!int.TryParse((string)fieldData["ID"], out IntTest))
            {
                throw new FormValidationException("Database value for 'ID' could not be parsed to a bool","ID");
            }
            if (!bool.TryParse((string)fieldData["Required"],out BoolTest))
            {
                throw new FormValidationException("Database value for 'Required' could not be parsed to a bool","Required");
            }
            if (!int.TryParse((string)fieldData["SortOrder"], out IntTest))
            {
                throw new FormValidationException("Database value for 'SortOrder' could not be parsed to a bool","SortOrder");
            }

            return true;
        }
    }
}
