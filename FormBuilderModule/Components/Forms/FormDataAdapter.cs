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
            comm.CommandText = "SELECT * FROM " + Config.TableNames["Forms"] + " WHERE FieldID=@FieldID";
            da.Fill(ds);
            var formData = ds.GetResults()[0];
            if (formData != null)
            {
                //Calls the GetTemplate member function and fills the InteralTemplate variable.
                InternalForm.ID = (int)formData["ID"];
                InternalForm.CreatedBy = (string)formData["CreatedBy"];
                InternalForm.CreatedDate = (DateTime)formData["CreatedDate"];
                InternalForm.Template = this.GetTemplate((int)formData["TemplateID"]);
                InternalForm.Sections = new List<Section>();
                InternalForm.Sections = InternalForm.Template.Sections;

                if (Recursive)
                {
                    LoadValues();
                }
            }

            return InternalForm;
        }

        public List<Form> GetForms(int Number)
        {
            ClearData();
            List<Form> Forms = new List<Form>();
            comm.CommandText = "SELECT ID FROM " + Config.TableNames["Forms"] + " WHERE FieldID=@FieldID";
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
                comm.CommandText = "INSERT INTO " + Config.TableNames["Forms"] + " (TemplateID,CreatedDate) VALUES (@TemplateID,@CreatedDate); SELECT SCOPE_IDENTITY();";
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
                    foreach (Value val in field.Values)
                    {
                        comm.Parameters.AddWithValue("@FormID", form.ID);
                        comm.Parameters.AddWithValue("@FieldID", field.ID);
                        comm.Parameters.AddWithValue("@Content", val.Content);

                        if (val.ID.HasValue)
                        {
                            comm.CommandText = "UPDATE " + Config.TableNames["Values"] + " SET FormID=@FormID, FieldID=@FieldID, Content=@Content WHERE ID=@ID";
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
                            comm.CommandText = "INSERT INTO " + Config.TableNames["Values"] + " (FormID, FieldID, Content) VALUES (@FormID, @FieldID, @Content); SELECT SCOPE_IDENTITY();";
                            int newID;
                            try
                            {
                                newID = (int)comm.ExecuteScalar();
                                val.ID = newID;
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

        private void LoadValues()
        {
            ClearData();
            comm.CommandText = "SELECT * FROM " + Config.TableNames["Values"] + " WHERE FormID=@FormID";
            da.Fill(ds);
            var valuesData = ds.GetResults();
            if (valuesData != null)
            {
                foreach (DataRow value in valuesData)
                {
                    int FieldID = (int)value["FieldID"];
                    Value v = new Value
                    {
                        ID = (int)value["ID"],
                        FormID = (int)value["FormID"],
                        FieldID = (int)value["FieldID"],
                        Content = (string)value["Content"]
                    };
                    //Find the section that contains the field id currently being processed, then get the field that the ID matches, and add the value to its Values
                    InternalForm.Sections.Where(sec => sec.Fields.Where(field => field.ID == FieldID).Count() >= 1).First().Fields.First(field => field.ID == FieldID).Values.Add(v);
                }
            }
        }
    }
}
