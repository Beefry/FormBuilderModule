 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace Beefry.FormBuilder
{
    public class Config
    {
        public static Dictionary<string,string> DefaultSettings = new Dictionary<string,string> {
            {"TablePrefix","bf_"} ,
            {"DatabaseName","beefryfb"},
            {"DatabaseHostname",""},
            {"DatabaseUsername",""},
            {"DatabasePassword",""}
        };
        public static Dictionary<string, string> TableNames = new Dictionary<string, string>
        {
            {"Forms","forms"},
            {"Fields","fields"},
            {"Options","options"}
        };
        private HttpServerUtility ServerContext;

        public Config(HttpServerUtility Server)
        {
            try
            {
                ConfigValidation();
                this.ServerContext = Server;
                Init();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ConfigValidation()
        {
            if (DefaultSettings["DatabaseName"] == "")
            {
                throw new Exception("Database name is not set");
            }
            if (DefaultSettings["DatabaseHostname"] == "")
            {
                throw new Exception("Database host name is not set");
            }
            if (DefaultSettings["DatabaseUsername"] == "")
            {
                throw new Exception("Database username is not set");
            }
        }

        private void Init()
        {
            try
            {
                this.InjectWebFiles();
                this.EnsureTableIntegrity();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InjectWebFiles()
        {
            string scriptPath = "~/Scripts";
            string templatePath = "~/Templates";
            //Check and make directory
            if (Directory.Exists(ServerContext.MapPath(scriptPath)))
            {
                
                try
                {
                    Directory.CreateDirectory(ServerContext.MapPath(scriptPath));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            //While developing/testing, delete these files on every run so that the freshest files can be imported.
            //TODO: get rid of the two lines below in production
            File.Delete(Path.Combine(ServerContext.MapPath(scriptPath), "app.js"));
            File.Delete(Path.Combine(ServerContext.MapPath(templatePath), "Builder.htm"));
            //Check main app file and create if it doesn't exist
            if (!File.Exists(Path.Combine(ServerContext.MapPath(scriptPath), "app.js")))
            {
                try
                {
                    File.WriteAllText(Path.Combine(ServerContext.MapPath(scriptPath), "app.js"), FormBuilderModule.Properties.Resources.app);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            //Check to see if template Path exists in host project directory. If not, create it.
            if (!Directory.Exists(ServerContext.MapPath(templatePath)))
            {
                try
                {
                    Directory.CreateDirectory(ServerContext.MapPath(templatePath));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            
            if (!File.Exists(Path.Combine(ServerContext.MapPath(templatePath), "Builder.htm")))
            {
                try
                {
                    File.WriteAllText(Path.Combine(ServerContext.MapPath(templatePath), "Builder.htm"), FormBuilderModule.Properties.Resources.Builder_template);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void EnsureTableIntegrity()
        {
            using (SqlConnection conn = new SqlConnection("Server="+DefaultSettings["DatabaseHostname"]+";Uid="+DefaultSettings["DatabaseUsername"]+";Pwd="+DefaultSettings["DatabasePassword"]+";"))
            {
                using (SqlCommand comm = new SqlCommand("", conn))
                {
                    //Check Database exists and create if not;
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM sys.databases WHERE name = @dbName)) BEGIN CREATE DATABASE " + DefaultSettings["DatabaseName"] + " END";
                    comm.Parameters.AddWithValue("@dbName", DefaultSettings["DatabaseName"]);
                    try
                    {
                        conn.Open();
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }

                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }

                    conn.ChangeDatabase(DefaultSettings["DatabaseName"]);

                    //Check if forms table exists and create if not
                    comm.Parameters.Clear();
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @dbName AND TABLE_NAME = @tableName))" +
                        " BEGIN CREATE TABLE " + TableNames["Forms"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, Label varchar(50), Description varchar(140), CreatedDate datetime); END";
                    comm.Parameters.AddWithValue("dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("tableName", TableNames["Forms"]);
                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //Check if fields table exists and create if not
                    comm.Parameters.Clear();
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @dbName AND TABLE_NAME = @tableName))" +
                        " BEGIN CREATE TABLE " + TableNames["Fields"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, FormID int, Label varchar(50), Type varchar(50), SortOrder int, Value varchar(MAX)); END";
                    comm.Parameters.AddWithValue("dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("tableName", TableNames["Fields"]);
                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //Check if options table exists and create if not
                    comm.Parameters.Clear();
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @dbName AND TABLE_NAME = @tableName))" +
                        " BEGIN CREATE TABLE " + TableNames["Options"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, FieldID int, Value varchar(50), SortOrder int); END";
                    comm.Parameters.AddWithValue("dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("tableName", TableNames["Options"]);
                    try
                    {
                        comm.ExecuteNonQuery();
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
