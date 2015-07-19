using Beefry;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Beefry.FormBuilder
{
    [JsonObject]
    public class Form
    {
        [JsonProperty]
        public int? ID;
        [JsonProperty]
        public DateTime CreatedDate;
        [JsonProperty]
        public string CreatedBy;
        [JsonProperty]
        public List<Section> Sections;
        [JsonProperty]
        public Template Template;

        public Form() : base() {}

        public Form(int ID) : base() 
        {
            Load(ID);
        }

        public void Load(int ID)
        {
            FormDataAdapter adapter = new FormDataAdapter();
            Form f = adapter.GetForm(ID);
            this.ID = f.ID;
            this.CreatedDate = f.CreatedDate;
            this.CreatedBy = f.CreatedBy;
            this.Template = f.Template;
            this.Sections = f.Template.Sections;
        }

        public void Save()
        {
            FormDataAdapter adapter = new FormDataAdapter();
            adapter.SaveForm(this);
        }
    }
}
