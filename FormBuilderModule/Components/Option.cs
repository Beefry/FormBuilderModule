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
        public int? FieldID { get; set; }
        public int SortOrder { get; set; }
        public string Value { get; set; }

        public Option(System.Data.DataRow optionData) : this() //In case we add functionality to Option()
        {
            try { this.ID = (int)optionData["ID"]; }
            catch (InvalidCastException ex) { throw new FormValidationException("Database value for ID could not be parsed", "ID"); }

            try { this.ID = (int)optionData["FieldID"]; }
            catch (InvalidCastException ex) { throw new FormValidationException("Database value for FieldID could not be parsed", "FieldID"); }

            try { this.ID = (int)optionData["SortOrder"]; }
            catch (InvalidCastException ex) { throw new FormValidationException("Database value for SortOrder could not be parsed", "SortOrder"); }

            this.Value = (string)optionData["Value"];
        }

        public Option()
        {

        }

        public bool Validate(System.Data.DataRow optionData)
        {
            int IntTest;
            bool BoolTest;
            DateTime DateTest;

            if (bool.TryParse((string)optionData["ID"], out BoolTest))
            {
                throw new FormValidationException("The database value for option ID could not be parsed", "ID");
            }
            if (bool.TryParse((string)optionData["SortOrder"], out BoolTest))
            {
                throw new FormValidationException("The database value for option ID could not be parsed", "SortOrder");
            }

            return true;
        }
    }
}
