using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using Beefry;

namespace Beefry.FormBuilder
{
    public enum FormMode
    {
        Edit,
        Fill
    }

    public class Form
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<Field> Fields { get; set; }
        public readonly bool IsValid { get; set; }
        private FormMode Mode { get; set; }
        public readonly List<string> InvalidFields { get; set; }

        public static readonly string FormBuilderDirective = "<formbuilder></formbuilder>";
        public static readonly string FormDisplayDirective = "<formdisplay></formdisplay>";

        public Form(FormMode mode)
        {
            this.Mode = mode;
            this.Fields = new List<Field>();
            this.IsValid = true;
            this.InvalidFields = new List<string>();
        }

        public void LoadForm(int ID) {
            this.ID = ID;
            getFormStructure();
        }

        /// <summary>
        /// Get the structure of a form from the database
        /// </summary>
        private void getFormStructure()
        {
            using (SqlConnection conn = new SqlConnection(Config.DBConnectionStringWithDB))
            {
                using (SqlCommand comm = new SqlCommand("", conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(comm);
                    DataSet ds = new DataSet();
                    comm.CommandText = "SELECT * FROM " + Config.TableNames["Forms"] + " WHERE ID=@ID;";
                    comm.Parameters.AddWithValue("@ID",this.ID);
                    try
                    {
                        da.Fill(ds);
                        if (ds.HasResults())
                        {
                            var formMeta = ds.Tables[0].Rows[0];
                            this.Name = (string)formMeta["Name"];
                            this.Description = (string)formMeta["Description"];
                            getFieldsStructure(comm);
                        }                        
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comm">The SqlCommand currently being used</param>
        private List<Field> getFieldsStructure(SqlCommand comm)
        {
            SqlDataAdapter da = new SqlDataAdapter(comm);
            DataSet ds = new DataSet();
            List<Field> Fields = new List<Field>();

            comm.Parameters.Clear();
            comm.CommandText = "SELECT * FROM " + Config.TableNames["Fields"] + " WHERE FormID=@FormID ORDER BY SortOrder";
            comm.Parameters.AddWithValue("@FormID", this.ID);
            try
            {
                da.Fill(ds);
                if (ds.HasResults())
                {
                    DataRowCollection fieldsData = ds.Tables[0].Rows;
                    foreach (DataRow field in fieldsData)
                    {
                        Field newField = new Field();
                        if (newField.Validate(field))
                        {
                            newField.ID = (int)field["ID"];
                            newField.FormID = this.ID;
                            newField.Label = (string)field["Label"];
                            newField.Required = (bool)field["Required"];
                            newField.SortOrder = (int)field["SortOrder"];
                            newField.Type = (string)field["Type"];
                            newField.Value = new Value();
                            Fields.Add(newField);
                            getOptionsStructure(comm,newField);
                        }
                    }
                }
            }
            catch (FormValidationException ex)
            {
                this.IsValid = false;
                this.InvalidFields.Add(ex.InvalidField);
                //throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Fields;
        }

        private List<Option> getOptionsStructure(SqlCommand comm,Field field) {
            comm.Parameters.Clear();
            comm.CommandText = "SELECT * FROM " + Config.TableNames["Options"] + " WHERE FieldID=@FieldID";
            comm.Parameters.AddWithValue("@FieldID", field.ID);

        }

        /// <summary>
        /// Get the values of a form based on the ID passed.
        /// </summary>
        /// <param name="id">The ID of the form to retreive data for</param>
        public void getFormContents(int id)
        {
            
        }

        public void saveForm()
        {
            try
            {
                if (Mode == FormMode.Edit)
                {
                    saveFormStructure();
                }
                else if(Mode == FormMode.Fill)
                {
                    saveFormContents();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void saveFormContents()
        {
            SqlConnection conn = new SqlConnection(Config.DBConnectionStringWithDB);
            SqlCommand comm = new SqlCommand("", conn);

            conn.Open();

            foreach(Field field in this.Fields) {
                //NOTE: this is currently only for 1-to-1 value/field pairing. Need to update later for multiple values per field (SELECT)
                if (field.Value.ID.HasValue)
                {
                    comm.CommandText = "UPDATE " + Config.TableNames["Values"] + " FieldID=@FieldID, Value=@Value WHERE ID=@ID";
                }
                else
                {
                    comm.CommandText = "INSERT INTO " + Config.TableNames["Values"] + " (FieldID, Value) VALUES (@FieldID, @Value); SELECT SCOPE_IDENTITY();";
                }
                
                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("@ID",field.Value.ID);
                comm.Parameters.AddWithValue("@FieldID", field.ID);
                comm.Parameters.AddWithValue("@Value", field.Value.Content);

                try
                {
                    if (field.Value.ID.HasValue)
                    {
                        field.Value.ID = Convert.ToInt32(comm.ExecuteScalar());
                    }
                    else
                    {
                        comm.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Save the contents of a form to the database
        /// </summary>
        public void saveFormStructure()
        {
            SqlConnection conn = new SqlConnection(Config.DBConnectionStringWithDB);
            SqlCommand comm = new SqlCommand();

            comm.Connection = conn;
            conn.Open();

            bool isInsert = false;
            //Update the form information
            //conn.BeginTransaction("formTransaction");
            //Reusing the parameters from the statement above.
            //TODO: do a check before updating/inserting?
            if (this.ID != null)
            {
                comm.CommandText = "UPDATE " + Config.TableNames["Forms"] + " SET ID=@ID, Label=@Label, Description=@Description WHERE ID=@ID";
            }
            else
            {
                comm.CommandText = "INSERT INTO " + Config.TableNames["Forms"] + " (Label, Description) VALUES (@Label,@Description);SELECT SCOPE_IDENTITY();";
                isInsert = true;
            }
            
            comm.Parameters.AddWithValue("@Label", this.Name);
            comm.Parameters.AddWithValue("@Description", this.Description);

            if (isInsert)
            {
                int result = Convert.ToInt32(comm.ExecuteScalar());
                this.ID = result;
            }
            else
            {
                comm.Parameters.AddWithValue("@ID", this.ID);
                comm.ExecuteNonQuery();
            }

            comm.Parameters.Clear();

            try {
                saveFieldStructure(comm);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void saveFieldStructure(SqlCommand comm)
        {
            #region Delete DB field entries not in current model
            comm.Parameters.Clear();
            //DELETE the fields that are currently not a part of the model
            comm.CommandText = "DELETE FROM " + Config.TableNames["Fields"] + " WHERE FormID=@FormID";
            string retainInQuery = " AND ID NOT IN (";
            bool needsUpdating = false;
            foreach (Field field in this.Fields)
            {
                if (field.ID.HasValue)
                {
                    retainInQuery += field.ID + ",";
                    needsUpdating = true;
                }
            }
            if (needsUpdating)
            {
                //remove the last comma and then append a closing parenthesis.
                comm.CommandText = comm.CommandText.Substring(0, comm.CommandText.Length - 2) + ");";
                comm.CommandText += retainInQuery;
            }

            comm.Parameters.AddWithValue("@FormID", this.ID);

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

            //Cycle through each field and update/insert depending on if it matches existing field IDs for this form ID
            #region Field INSERT/UPDATE queries

            foreach (Field field in this.Fields)
            {
                field.FormID = this.ID;
                string fieldUpdateQuery = "UPDATE " + Config.TableNames["Fields"] + " SET FormID=@FormID,ID=@ID,Label=@Label,"
                        + "Required=@Required,SortOrder=@SortOrder,Type=@Type WHERE ID=@ID";
                string fieldInsertQuery = "INSERT INTO " + Config.TableNames["Fields"] + " (FormID, Label, Required, SortOrder, Type) "
                    + "VALUES (@FormID, @Label, @Required, @SortOrder, @Type); SELECT SCOPE_IDENTITY();";

                if (field.ID.HasValue)
                {
                    comm.CommandText = fieldUpdateQuery;
                }
                else
                {
                    comm.CommandText = fieldInsertQuery;
                }

                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("FormID", this.ID);
                comm.Parameters.AddWithValue("Label", field.Label);
                comm.Parameters.AddWithValue("Required", field.Required);
                comm.Parameters.AddWithValue("SortOrder", field.SortOrder);
                comm.Parameters.AddWithValue("Type", field.Type);

                if (field.ID.HasValue)
                {
                    try
                    {
                        comm.Parameters.AddWithValue("ID", field.ID);
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
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }
                }

                try
                {
                    if ((field.Type == "select" || field.Type == "checkbox" || field.Type == "radio") && field.Options.Count > 0)
                    {
                        saveOptionStructure(field, comm);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            #endregion
        }

        private void saveOptionStructure(Field field, SqlCommand comm)
        {

            string retainInQuery = " AND ID NOT IN (";
            bool needsUpdating = false;
            comm.CommandText = "DELETE FROM " + Config.TableNames["Options"] + " WHERE FieldID=@FieldID";
            comm.Parameters.Clear();
            foreach (Option option in field.Options)
            {
                if (option.ID.HasValue)
                {
                    //TODO: Parameterize this
                    retainInQuery += option.ID + ",";
                    needsUpdating = true;
                }
            }

            if (needsUpdating)
            {
                comm.CommandText = comm.CommandText.Substring(0, comm.CommandText.Length - 2) + ");";
                comm.CommandText += retainInQuery;
            }

            try
            {
                comm.Parameters.AddWithValue("@FieldID", field.ID);
                comm.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ex;
            }

            //Cycle through each option and update/insert depending on if it matches existing option IDs for this field ID
            foreach (Option option in field.Options)
            {
                option.FieldID = field.ID;
                string optionUpdateQuery = "UPDATE " + Config.TableNames["Options"] + " SET ID=@ID, FieldID=@FieldID, Value=@Value, SortOrder=@SortOrder";
                string optionInsertQuery = "INSERT INTO " + Config.TableNames["Options"] + " (FieldID, Value, SortOrder) "
                    + "VALUES (@FieldID, @Value, @SortOrder); SELECT SCOPE_IDENTITY();";

                if (option.ID.HasValue)
                {
                    comm.CommandText = optionUpdateQuery;
                }
                else
                {
                    comm.CommandText = optionInsertQuery;
                }

                comm.Parameters.Clear();
                comm.Parameters.AddWithValue("FieldID", field.ID);
                comm.Parameters.AddWithValue("Value", option.Value);
                comm.Parameters.AddWithValue("SortOrder", option.SortOrder);

                if (option.ID.HasValue)
                {
                    try
                    {
                        comm.Parameters.AddWithValue("ID", option.ID);
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
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

        private List<int> getOptionIDs(int fieldID, SqlCommand comm) {
            SqlDataAdapter da = new SqlDataAdapter(comm);
            DataSet ds = new DataSet();
            comm.CommandText = "SELECT ID FROM " + Config.TableNames["Options"] + " WHERE FieldID=@FieldID;";
            comm.Parameters.AddWithValue("@FieldID", fieldID);
            da.Fill(ds);
            List<int> updateIDs = new List<int>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    updateIDs.Add((int)ds.Tables[0].Rows[0]["ID"]);
                }
                else
                {
                    //No results
                }
            }
            else
            {
                //Throw error
            }
            return updateIDs;
        }

        private List<int> getFieldIDs(SqlCommand comm)
        {
            SqlDataAdapter da = new SqlDataAdapter(comm);
            DataSet ds = new DataSet();
            comm.CommandText = "SELECT ID FROM " + Config.TableNames["Fields"] + " WHERE FormID=@FormID;";
            comm.Parameters.AddWithValue("@field_table", Config.TableNames["Fields"]);
            comm.Parameters.AddWithValue("@FormID", this.ID);
            da.Fill(ds);
            List<int> updateIDs = new List<int>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    updateIDs.Add((int)ds.Tables[0].Rows[0]["ID"]);
                }
                else
                {
                    //No results
                }
            }
            else
            {
                //Throw error
            }
            return updateIDs;
        }
    }
}
