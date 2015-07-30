﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beefry.FormBuilder
{
    public class Value
    {
        public int? ID { get; set; }
        public int? FormID { get; set; }
        public int? FieldID { get; set; }
        //TODO: break this into a separate object for multi-value Field types
        public int? OptionID { get; set; }
        //TODO: change this for multiple datatypes. More for database storage usage, currently using nvarchar(150), when this could be streamlined depending on field type.
        public string Content { get; set; }
    }
}
