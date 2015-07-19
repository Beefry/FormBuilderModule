using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Beefry.FormBuilder
{
    public class TemplateDataAdapter : BeefryDataAdapter
    {
        protected Template InternalTemplate;
        /// <summary>
        /// Constructor for the FormDataAdapter. Initiates all communication to the database and all data aquisition.
        /// </summary>
        /// <param name="refForm">the Form that is used to make CRUD actions on. SHOULD be passed by reference by default.</param>
        public TemplateDataAdapter() : base() {}

        public TemplateCollection GetTemplates(int Number = 10)
        {
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            TemplateCollection Forms = new TemplateCollection();
            comm.CommandText = "SELECT TOP(@num) ID FROM " + Config.TableNames["Templates"];
            comm.Parameters.AddWithValue("@num", Number);
            try
            {
                da.Fill(ds);
                var ids = ds.GetResults();
                if (ds != null)
                {

                    foreach (DataRow id in ids)
                    {
                        Template formRef = new Template((int)id["ID"]);
                        Forms.Add(formRef);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error trying to open database connection in GetTopForms: " + ex.Message, ex);
            }

            return Forms;
        }

        /// <summary>
        /// Adapter function to get data from DB and load it into a From object.
        /// </summary>
        /// <param name="ID">ID of the form to get.</param>
        /// <param name="Recursive">Defaults to true. If set to false, only gets the data for the form and nothing further (IE the sections).</param>
        public Template GetTemplate(int ID, bool Recursive = true)
        {
            try
            {
                conn.Open();
            }
            catch (SqlException ex)
            {
                throw new Exception("Error trying to open database for FormDataAdapter: " + ex.Message, ex);
            }

            comm.CommandText = "SELECT * FROM " + Config.TableNames["Forms"] + " WHERE ID=@ID;";
            comm.Parameters.AddWithValue("@ID", InternalTemplate.ID);
            try
            {
                da.Fill(ds);
                var formMeta = ds.GetResults();
                if (formMeta != null)
                {
                    InternalTemplate.Name = (string)formMeta[0]["Name"];
                    InternalTemplate.Description = (string)formMeta[0]["Description"];
                    InternalTemplate.Sections = new List<Section>();
                    if (Recursive)
                    {
                        GetSections(Recursive);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in getFormStructure(): " + ex.Message);
            }
            //Close dat connection. No longer needed, bro!
            conn.Close();

            return InternalTemplate;
        }

        /// <summary>
        /// Save the contents of a form to the database
        /// </summary>
        public void SaveTemplate(Template template)
        {
            this.ClearData();
            //Update the form information
            //conn.BeginTransaction("formTransaction");
            //Reusing the parameters from the statement above.
            //TODO: do a check before updating/inserting?
            if (InternalTemplate.ID.HasValue)
            {
                comm.CommandText = "UPDATE " + Config.TableNames["Templates"] + " SET Name=@Name, Description=@Description WHERE ID=@ID";
            }
            else
            {
                comm.CommandText = "INSERT INTO " + Config.TableNames["Templates"] + " (Name, Description) VALUES (@Name,@Description);SELECT SCOPE_IDENTITY();";
            }

            comm.Parameters.AddWithValue("@Name", InternalTemplate.Name);
            comm.Parameters.AddWithValue("@Description", InternalTemplate.Description);

            if (!InternalTemplate.ID.HasValue)
            {
                int result = Convert.ToInt32(comm.ExecuteScalar());
                InternalTemplate.ID = result;
            }
            else
            {
                comm.Parameters.AddWithValue("@ID", InternalTemplate.ID);
                comm.ExecuteNonQuery();
            }

            try
            {
                SaveSections();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region private Methods

        #region getters
        private void GetSections(bool Recurse)
        {
            if (Recurse)
            {
                comm.Parameters.Clear();
                comm.CommandText = "SELECT * FROM " + Config.TableNames["Sections"] + " WHERE FormID=@ID;";
                comm.Parameters.AddWithValue("@ID", InternalTemplate.ID);
                ds.Clear();
                da.Fill(ds);
                var sectionData = ds.GetResults();
                if (sectionData != null)
                {
                    foreach (DataRow section in sectionData)
                    {
                        Section s = new Section();
                        s.ID = (int?)section["ID"];
                        s.Name = (string)section["Name"];
                        s.Fields = GetFields(s);
                        InternalTemplate.Sections.Add(s);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comm">The SqlCommand currently being used</param>
        private List<Field> GetFields(Section section)
        {
            List<Field> Fields = new List<Field>();

            comm.Parameters.Clear();
            ds.Clear();

            comm.CommandText = "SELECT * FROM " + Config.TableNames["Fields"] + " WHERE SectionID=@SectionID ORDER BY SortOrder";
            comm.Parameters.AddWithValue("@SectionID", section.ID);
            try
            {
                da.Fill(ds);
                var fieldsData = ds.GetResults();
                if (fieldsData != null)
                {
                    foreach (DataRow field in fieldsData)
                    {
                        try
                        {
                            Field newField = new Field(field);
                            newField.SectionID = section.ID;
                            newField.ID = (int)field["ID"];
                            newField.Label = (string)field["ID"];
                            newField.Required = (bool)field["Requierd"];
                            newField.SortOrder = (int)field["SortOrder"];
                            newField.Type = (string)field["Type"];
                            newField.Options = GetOptions(newField);
                            //If the mode requires field values, then get the values
                            Fields.Add(newField);
                        }
                        catch (FormValidationException ex)
                        {
                            //InternalForm._IsValid = false;
                            //InternalForm._InvalidFields.Add(ex.InvalidField);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
            catch (FormValidationException ex)
            {
                //InternalForm._IsValid = false;
                //InternalForm._InvalidFields.Add(ex.InvalidField);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in getFieldsStructure(): " + ex.Message);
            }

            return Fields;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private List<Option> GetOptions(Field field)
        {
            ds.Clear();
            comm.Parameters.Clear();
            List<Option> Options = new List<Option>();
            comm.CommandText = "SELECT * FROM " + Config.TableNames["Options"] + " WHERE FieldID=@FieldID";
            comm.Parameters.AddWithValue("@FieldID", field.ID);
            try
            {
                da.Fill(ds);
                var optionData = ds.GetResults();
                if (optionData != null)
                {
                    foreach (DataRow option in optionData)
                    {
                        Option NewOption = new Option(option);
                        NewOption.FieldID = field.ID;
                        NewOption.ID = (int?)option["ID"];
                        NewOption.SortOrder = (int)option["SortOrder"];
                        NewOption.Value = (string)option["Value"];
                        Options.Add(NewOption);
                    }
                }
            }
            catch (FormValidationException vEx)
            {
                //InternalForm._IsValid = false;
                //InternalForm._InvalidFields.Add(vEx.InvalidField);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in getOptionsStructure(): " + ex.Message);
            }

            return Options;

        }

        #endregion

        #region setters
        private void SaveSections()
        {
            this.ClearData();

            //DELETE the fields that are currently not a part of the model
            comm.CommandText = "DELETE FROM " + Config.TableNames["Sections"] + " WHERE FormID=@FormID";
            if (InternalTemplate.Sections.Count > 0)
            {
                string retainInQuery = " AND ID NOT IN (";
                foreach (Section section in InternalTemplate.Sections)
                {
                    if (section.ID.HasValue)
                    {
                        retainInQuery += section.ID + ",";
                    }
                }
                comm.CommandText += retainInQuery;
                comm.CommandText = comm.CommandText.Substring(0, comm.CommandText.Length - 1) + ");";
                comm.Parameters.AddWithValue("@FormID", InternalTemplate.ID);
            }

            try
            {
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(comm.CommandText);
                throw ex;
            }

            this.ClearData();
            //TODO: Batch these statements
            foreach (Section section in InternalTemplate.Sections)
            {
                comm.Parameters.AddWithValue("@Name", section.Name);
                comm.Parameters.AddWithValue("@Order", section.Order);

                if (section.ID.HasValue)
                {
                    comm.CommandText = "UPDATE " + Config.TableNames["Sections"] + " SET Name=@Name, Order=@Order WHERE ID=@ID";
                    comm.Parameters.AddWithValue("@ID", section.ID);
                    comm.ExecuteNonQuery();
                }
                else
                {
                    comm.CommandText = "INSERT INTO " + Config.TableNames["Sections"] + " (Name, Order) VALUES (@Name,@Order);SELECT SCOPE_IDENTITY();";
                    int newID = (int)comm.ExecuteScalar();
                    section.ID = newID;
                }
                SaveFields(section);
            }
        }

        private void SaveFields(Section section)
        {
            #region Delete DB field entries not in current model
            comm.Parameters.Clear();
            //DELETE the fields that are currently not a part of the model
            comm.CommandText = "DELETE FROM " + Config.TableNames["Fields"] + " WHERE FormID=@FormID";
            if (section.Fields.Count > 0)
            {
                string retainInQuery = " AND ID NOT IN (";
                foreach (Field field in section.Fields)
                {
                    if (field.ID.HasValue)
                    {
                        retainInQuery += field.ID + ",";
                    }
                }
                //remove the last comma and then append a closing parenthesis.
                comm.CommandText += retainInQuery;
                comm.CommandText = comm.CommandText.Substring(0, comm.CommandText.Length - 1) + ");";

                comm.Parameters.AddWithValue("@FormID", InternalTemplate.ID);
            }
            try
            {
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(comm.CommandText);
                throw ex;
            }

            #endregion

            //Cycle through each field and update/insert depending on if it matches existing field IDs for InternalForm form ID
            #region Field INSERT/UPDATE queries

            foreach (Field field in section.Fields)
            {
                this.ClearData();
                field.SectionID = section.ID;
                comm.Parameters.AddWithValue("FormID", field.SectionID);
                comm.Parameters.AddWithValue("Label", field.Label);
                comm.Parameters.AddWithValue("Required", field.Required);
                comm.Parameters.AddWithValue("SortOrder", field.SortOrder);
                comm.Parameters.AddWithValue("Type", field.Type);

                if (field.ID.HasValue)
                {
                    comm.CommandText = "UPDATE " + Config.TableNames["Fields"] + " SET FormID=@FormID,Label=@Label,"
                        + "Required=@Required,SortOrder=@SortOrder,Type=@Type WHERE ID=@ID";
                    comm.Parameters.AddWithValue("ID", field.ID);
                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        field.ID = Convert.ToInt32(comm.ExecuteScalar());
                        comm.CommandText = "INSERT INTO " + Config.TableNames["Fields"] + " (FormID, Label, Required, SortOrder, Type) "
                        + "VALUES (@FormID, @Label, @Required, @SortOrder, @Type); SELECT SCOPE_IDENTITY();";
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }
                }

                SaveOptions(field);
            }
            #endregion
        }

        private void SaveOptions(Field field)
        {
            if ((field.Type == "select" || field.Type == "checkbox" || field.Type == "radio") && field.Options.Count > 0)
            {
                this.ClearData();
                comm.Parameters.AddWithValue("@FieldID", field.ID);
                if (field.Options.Count > 0)
                {
                    string retainInQuery = " AND ID NOT IN (";
                    comm.CommandText = "DELETE FROM " + Config.TableNames["Options"] + " WHERE FieldID=@FieldID";
                    foreach (Option option in field.Options)
                    {
                        //TODO: Parameterize InternalForm
                        retainInQuery += option.ID + ",";
                    }
                    comm.CommandText += retainInQuery;
                    comm.CommandText = comm.CommandText.Substring(0, comm.CommandText.Length - 1) + ");";
                }

                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw ex;
                }

                //Cycle through each option and update/insert depending on if it matches existing option IDs for InternalForm field ID
                foreach (Option option in field.Options)
                {
                    option.FieldID = field.ID;

                    comm.Parameters.Clear();
                    comm.Parameters.AddWithValue("FieldID", field.ID);
                    comm.Parameters.AddWithValue("Value", option.Value);
                    comm.Parameters.AddWithValue("SortOrder", option.SortOrder);

                    if (option.ID.HasValue)
                    {
                        comm.CommandText = "UPDATE " + Config.TableNames["Options"] + " SET " +
                        " FieldID=@FieldID," +
                        " Value=@Value," +
                        " SortOrder=@SortOrder" +
                        " WHERE ID=@ID";
                        comm.Parameters.AddWithValue("ID", option.ID);
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
                        comm.CommandText = "INSERT INTO " + Config.TableNames["Options"] +
                        " (FieldID, Value, SortOrder) " +
                        "VALUES (@FieldID, @Value, @SortOrder); SELECT SCOPE_IDENTITY();";
                        try
                        {
                            option.ID = Convert.ToInt32(comm.ExecuteScalar());
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
