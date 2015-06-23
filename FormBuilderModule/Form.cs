using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SqlClient;

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
        private FormMode Mode { get; set; }

        public static readonly string FormBuilderDirective = "<formbuilder></formbuilder>";
        public static readonly string FormDisplayDirective = "<formdisplay></formdisplay>";

        public Form(FormMode mode)
        {
            this.Mode = mode;
            this.Fields = new List<Field>();
        }

        /// <summary>
        /// Get the structure of a form from the database
        /// </summary>
        public void getForm()
        {
            
        }

        /// <summary>
        /// Get the values of a form based on the ID passed.
        /// </summary>
        /// <param name="id">The ID of the form to retreive data for</param>
        public void getFormContents(int id)
        {
            
        }

        /// <summary>
        /// Save the contents of a form to the database
        /// </summary>
        public void saveForm()
        {
            SqlConnection conn = new SqlConnection("");
            SqlCommand comm = new SqlCommand();

            comm.Connection = conn;
            conn.Open();

            bool isInsert = false;
            //Update the form information
            //conn.BeginTransaction("formTransaction");
            //Reusing the parameters from the statement above.
            //TODO: do a check before updating/inserting?
            if (this.ID == null)
            {
                comm.CommandText = "UPDATE @form_table SET ID=@ID, Name=@Name, Description=@Description WHERE ID=@ID";
            }
            else
            {
                comm.CommandText = "INSERT INTO @form_table (ID, Name, Description) VALUES (@ID,@Name,@Description);SELECT SCOPE_IDENTITY();";
                isInsert = true;
            }
            comm.Parameters.AddWithValue("@ID", this.ID);
            comm.Parameters.AddWithValue("@Name", this.Name);
            comm.Parameters.AddWithValue("@Description", this.Description);

            if (isInsert)
            {
                comm.ExecuteNonQuery();
            }
            else
            {
                this.ID = (int)comm.ExecuteScalar();
            }

            comm.Parameters.Clear();

            List<int> currentFieldIDs = getFieldIDs(comm);

            foreach(Field field in this.Fields) {
                try 
                { 
                    saveFields(comm);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void saveFields(SqlCommand comm)
        {
            comm.Parameters.Clear();
            //DELETE the fields that are currently not a part of the model
            comm.CommandText = "DELETE FROM @field_table WHERE form_id=@form_id AND ID NOT IN (";

            List<int> updateIDs = getFieldIDs(comm);

            foreach (Field field in this.Fields)
            {
                //Need to parameterize this.
                comm.CommandText += field.ID + ",";
            }

            //remove the last comma and then append a closing parenthesis.
            comm.CommandText = comm.CommandText.Substring(0, comm.CommandText.Length - 2) + ");";
            comm.Parameters.AddWithValue("@field_table", Config.TableNames["Fields"]);
            comm.Parameters.AddWithValue("@form_id", this.ID);

            try
            {
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(comm.CommandText);
                throw ex;
            }
            //Cycle through each field and update/insert depending on if it matches existing field IDs for this form ID
            foreach (Field field in this.Fields)
            {
                bool wasInsert = false;
                if (field.ID.HasValue && updateIDs.Contains(field.ID.Value))
                {
                    comm.CommandText = "UPDATE @field_table SET FormID=@FormID,ID=@ID,Label=@Label,"
                        + "Required=@Required,SortOrder=@SortOrder,Type=@Type WHERE ID=@ID";
                }
                else
                {
                    comm.CommandText = "INSERT INT @field_table (FormID, ID, Label, Name, Required, SortOrder, Type) "
                    + "VALUES (@FormID, @ID, @Label, @Required, @SortOrder, @Type); SELECT SCOPE_IDENTITY();";
                    wasInsert = true;
                }

                comm.Parameters.AddWithValue("field_table", Config.DefaultSettings["FormTableName"]);
                comm.Parameters.AddWithValue("FormID", this.ID);
                comm.Parameters.AddWithValue("ID", field.ID);
                comm.Parameters.AddWithValue("Label", field.Label);
                comm.Parameters.AddWithValue("Required", field.Required);
                comm.Parameters.AddWithValue("SortOrder", field.SortOrder);
                comm.Parameters.AddWithValue("Type", field.Type);

                if (wasInsert)
                {
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
                        field.ID = (int)comm.ExecuteScalar();
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }
                    
                }

                try
                { 
                    saveOptions(field, comm);
                }
                catch (Exception ex)
                {
                    throw ex;
                }                
            }
        }

        private void saveOptions(Field field, SqlCommand comm)
        {
            if (field.Type == "select" || field.Type == "checkbox" || field.Type == "radio")
            {
                if (field.Options.Count > 0)
                {
                    //Get List of non-updated
                    List<int> optionUpdatedIDs = new List<int>();
                    comm.CommandText = "DELETE FROM @OptionTable WHERE FieldID=@FieldID AND ID NOT IN (";
                    //TODO: Parameterize this
                    foreach (Option option in field.Options)
                    {
                        comm.CommandText += option.ID + ",";
                    }
                    comm.CommandText = comm.CommandText.Substring(0, comm.CommandText.Length - 2) + ");";
                    comm.Parameters.AddWithValue("@OptionTable", Config.TableNames["OptionTableName"]);
                    comm.Parameters.AddWithValue("@FieldID", field.ID);

                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }

                    //Cycle through each option and update/insert depending on if it matches existing option IDs for this field ID
                    foreach (Option option in field.Options)
                    {
                        
                        if (field.ID.HasValue && optionUpdatedIDs.Contains(field.ID.Value))
                        {
                            comm.CommandText = "UPDATE @OptionsTable SET ID=@ID, FieldID=@FieldID, Value=@Value, SortOrder=@SortOrder";
                        }
                        else
                        {
                            comm.CommandText = "INSERT INT @OptionsTable (ID, FieldID, Value, SortOrder) "
                            + "VALUES (@ID, @FieldID, @Value, @SortOrder);";
                        }

                        comm.Parameters.AddWithValue("field_table", Config.DefaultSettings["FormTableName"]);
                        comm.Parameters.AddWithValue("ID", option.ID);
                        comm.Parameters.AddWithValue("FieldID", field.ID);
                        comm.Parameters.AddWithValue("Value", option.Value);
                        comm.Parameters.AddWithValue("SortOrder", option.SortOrder);

                        comm.ExecuteNonQuery();
                    }
                }
                else
                {
                    comm.CommandText = "DELETE FORM @option_table WHERE ID=@ID";
                }
                comm.Parameters.AddWithValue("@option_table", Config.DefaultSettings["OptionsTableName"]);
                comm.Parameters.AddWithValue("@ID", field.ID);
                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
            }
        }

        private List<int> getOptionIDs(int fieldID, SqlCommand comm) {
            SqlDataAdapter da = new SqlDataAdapter(comm);
            DataSet ds = new DataSet();
            comm.CommandText = "SELECT ID FROM @options_table WHERE FieldID=@FieldID;";
            comm.Parameters.AddWithValue("@options_table", Config.TableNames["OptionsTableName"]);
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
            comm.CommandText = "SELECT ID FROM @field_table WHERE FormID=@FormID;";
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
