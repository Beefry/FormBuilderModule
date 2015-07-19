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

namespace Beefry.FormBuilder
{
    public class Template
    {
        private bool _IsValid;
        private List<string> _InvalidFields;

        public int? ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<Section> Sections { get; set; }

        [JsonIgnore]
        public bool IsValid { get { return _IsValid; } }
        [JsonIgnore]
        public List<string> InvalidFields { get { return _InvalidFields; } }

        //For now, this needs to be updated manually based on what is used in the anguar app.
        public static readonly string FormBuilderDirective = "<formbuilder></formbuilder>";
        public static readonly string FormBuilderEditDirective = "<formbuilder id='{0}'></formbuilder>";
        public static readonly string FormDisplayDirective = "<formdisplay></formdisplay>";
        public static readonly string FormDisplayEditDirective = "<formdisplay id='{0}'></formdisplay>";

        #region Constructors

        public Template()
        {
            this.Sections = new List<Section>();
            this._IsValid = true;
            this._InvalidFields = new List<string>();
        }

        public Template(int LoadID)
            : this()
        {
            this.Load(LoadID);
        }
        #endregion

        #region public Methods
        public void Load(int ID, bool Recursive = false)
        {
            this.ID = ID;
            //getFormStructure(comm);
            TemplateDataAdapter adapter = new TemplateDataAdapter();
            Template newTemp = adapter.GetTemplate(ID, Recursive);
            this.ID = newTemp.ID;
            this.Description = newTemp.Description;
            this.CreatedDate = newTemp.CreatedDate;
            this.Name = newTemp.Name;
            this.Sections = newTemp.Sections;
        }

        public void Save()
        {
            //TODO: validation before attempting to save
            try
            {
                TemplateDataAdapter adapter = new TemplateDataAdapter();
                adapter.SaveTemplate(this);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
