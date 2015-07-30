using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Beefry.FormBuilder
{
    public class FormDataAdapter : TemplateDataAdapter
    {
        private Form InternalForm;

        public FormDataAdapter() : base() 
        {
            this.InternalForm = new Form();
        }

        public Form GetForm(int ID, bool Recursive = true)
        {
            ClearData();
            comm.CommandText = "SELECT * FROM " + Config.TableNames["Forms"] + " WHERE ID=@ID";
            comm.Parameters.AddWithValue("@ID", ID);
            da.Fill(ds);
            var formData = ds.GetResults();
            if (formData != null)
            {
                //Calls the GetTemplate member function and fills the InteralTemplate variable.
                InternalForm.ID = (int)formData[0]["ID"];
                InternalForm.CreatedBy = (string)formData[0]["CreatedBy"];
                InternalForm.CreatedDate = (DateTime)formData[0]["CreatedDate"];
                InternalForm.Template = this.GetTemplate((int)formData[0]["TemplateID"]);
                InternalForm.Sections = new List<Section>();
                InternalForm.Sections = InternalForm.Template.Sections;

                if (Recursive)
                {
                    LoadValues();
                }
            }
            else
            {
                throw new Exception("No results found for the ID given.");
            }

            return InternalForm;
        }

        public List<Form> GetForms(int Number = 10)
        {
            ClearData();
            List<Form> Forms = new List<Form>();
            comm.CommandText = "SELECT TOP(@num) ID FROM " + Config.TableNames["Forms"];
            comm.Parameters.AddWithValue("@num", Number);
            da.Fill(ds);
            var formIDs = ds.GetResults();
            if (formIDs != null)
            {
                foreach (DataRow id in formIDs)
                {
                    Forms.Add(new Form((int)id["ID"]));
                }
            }

            return Forms;
        }

        public void SaveForm(Form Form)
        {
            ClearData();

            comm.Parameters.AddWithValue("@TemplateID", Form.Template.ID);

            if (Form.ID.HasValue)
            {
                comm.CommandText = "UPDATE " + Config.TableNames["Forms"] + " SET TemplateID=@TemplateID WHERE ID=@ID";
                comm.Parameters.AddWithValue("@ID",Form.ID);
                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                //TODO: add CreatedBy field once permissions and users are added to the system.
                comm.CommandText = "INSERT INTO " + Config.TableNames["Forms"] + " (TemplateID,CreatedDate) VALUES (@TemplateID,@CreatedDate); SELECT CAST(SCOPE_IDENTITY() as int);";
                comm.Parameters.AddWithValue("@CreatedDate",DateTime.Now);

                try
                {
                    int newID = (int)comm.ExecuteScalar();
                    Form.ID = newID;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            SaveValues(Form);
        }

        private void SaveValues(Form form)
        {
            ClearData();
            //TODO: DELETE values that no longer exist DELETE WHERE ID NOT IN ();
            foreach (Section sec in form.Sections)
            {
                foreach (Field field in sec.Fields)
                {
                    field.SectionID = sec.ID;
                    if (field.Type != "text")
                    {
                        foreach (Value val in field.Values)
                        {
                            if (val.Content != null)
                            {
                                val.FormID = form.ID;
                                val.FieldID = field.ID;
                                comm.Parameters.Clear();
                                comm.Parameters.AddWithValue("@Content", val.Content);

                                if (val.ID.HasValue)
                                {
                                    comm.CommandText = "UPDATE " + Config.TableNames["Values"] + " SET Content=@Content WHERE ID=@ID";
                                    comm.Parameters.AddWithValue("@ID", val.ID);
                                    try
                                    {
                                        comm.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                                else
                                {
                                    comm.Parameters.AddWithValue("@FormID", val.FormID);
                                    comm.Parameters.AddWithValue("@FieldID", val.FieldID);
                                    comm.Parameters.AddWithValue("@OptionID", val.OptionID.HasValue ? val.OptionID.Value.ToString() : "");
                                    comm.CommandText = "INSERT INTO " + Config.TableNames["Values"] + " (FormID, FieldID, OptionID, Content) VALUES (@FormID, @FieldID, @OptionID, @Content); SELECT CAST(SCOPE_IDENTITY() as int);";
                                    try
                                    {
                                        val.ID = (int)comm.ExecuteScalar();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadValues()
        {
            ClearData();
            DataSet ValueDS = new DataSet();
            comm.CommandText = "SELECT * FROM " + Config.TableNames["Values"] + " WHERE FormID=@FormID";
            comm.Parameters.AddWithValue("@FormID", InternalForm.ID);
            da.Fill(ValueDS);
            var valuesData = ValueDS.GetResults();
            if (valuesData != null)
            {
                foreach (DataRow value in valuesData)
                {
                    var FieldID = value["FieldID"];
                    Value v = new Value
                    {
                        ID = (int)value["ID"],
                        FormID = (int)value["FormID"],
                        OptionID = (int)value["OptionID"],
                        FieldID = (int)value["FieldID"],
                        Content = (string)value["Content"]
                    };
                    //Find the section that contains the field id currently being processed, then get the field that the ID matches
                    Field RelatedField = InternalForm.Sections.Where(sec => sec.Fields.Where(field => field.ID == v.FieldID).Count() >= 1).First().Fields.First(field => field.ID == v.FieldID);
                    //Find the specific Value that's been set up already and load the DB information into it.
                    Value RelatedValue = RelatedField.Values.First(val => val.FieldID == v.FieldID);
                    RelatedValue.FormID = v.FormID;
                    RelatedValue.FieldID = v.FieldID;
                    RelatedValue.OptionID = v.OptionID;
                    RelatedValue.Content = v.Content;
                }
            }
        }
    }
}
