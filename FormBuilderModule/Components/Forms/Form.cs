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

        public static readonly string FormDisplayDirective = "<formdisplayer></formdisplayer>";
        public static readonly string FormDisplayEditDirective = "<formdisplayeredit fid='{0}'></formdisplayeredit>";
        public static readonly string FormDisplayViewDirective = "<formdisplayerview fid='{0}'></formdisplayerview>";
        public static readonly string FormDisplayNewDirective = "<formdisplayeredit tid='{0}'></formdisplayeredit>";

        public Form() : base() {}

        public Form(int ID) : base() 
        {
            Load(ID);
        }

        public void New(int TemplateID)
        {
            TemplateDataAdapter adapter = new TemplateDataAdapter();
            try
            {
                this.Template = adapter.GetTemplate(TemplateID);
            }
            catch (Exception ex)
            {
                throw new Exception("Error trying to load teamplate in Template.New(). See inner exception for details.", ex);
            }
            try
            {
                this.Sections = Template.Sections;
            }
            catch (Exception ex)
            {
                throw new Exception("Error trying to load form in Template.New(). See inner exception for details.", ex);
            }            
        }

        public void Load(int ID)
        {
            FormDataAdapter adapter = new FormDataAdapter();
            try
            {
                Form f = adapter.GetForm(ID);
                this.ID = f.ID;
                this.CreatedDate = f.CreatedDate;
                this.CreatedBy = f.CreatedBy;
                this.Template = f.Template;
                this.Sections = f.Template.Sections;
            }
            catch (Exception ex)
            {
                throw new Exception("Error trying to load form in Template.Load(). See inner exception for details.", ex);
            }
        }

        public void Save()
        {
            FormDataAdapter adapter = new FormDataAdapter();
            adapter.SaveForm(this);
        }
    }
}
