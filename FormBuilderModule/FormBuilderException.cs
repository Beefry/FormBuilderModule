using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beefry.FormBuilder
{
    class FormValidationException : Exception
    {
        private string _InvalidField;
        public string InvalidField { get { return _InvalidField; } }
        public FormValidationException(string Message,string Field):base(message:Message) {
            this._InvalidField = Field;
        }
    }
}
