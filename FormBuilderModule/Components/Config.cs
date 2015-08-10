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
        public readonly bool isDebug;
        private HttpServerUtility ServerContext;

        public static Dictionary<string,string> DefaultSettings = new Dictionary<string,string> {
            {"TablePrefix","bf_"} ,
            {"DatabaseName","beefryfb"},
            {"DatabaseHostname",""},
            {"DatabaseUsername",""},
            {"DatabasePassword",""},
            {"DatabaseSchema","dbo"}
        };
        public static Dictionary<string, string> TableNamesSansSchema
        {
            get
            {
                return new Dictionary<string, string> {
                {"Templates",DefaultSettings["TablePrefix"]+"templates"},
                {"Forms",DefaultSettings["TablePrefix"]+"forms"},
                {"Fields",DefaultSettings["TablePrefix"]+"fields"},
                {"Options",DefaultSettings["TablePrefix"]+"options "},
                {"Values",DefaultSettings["TablePrefix"]+"values"},
                {"Sections",DefaultSettings["TablePrefix"]+"sections"},
                };
            }
        }

        public static Dictionary<string, string> TableNames
        {
            get
            {
                return new Dictionary<string, string> {
                {"Templates",DefaultSettings["DatabaseSchema"]+"."+DefaultSettings["TablePrefix"]+"templates"},
                {"Forms",DefaultSettings["DatabaseSchema"]+"."+DefaultSettings["TablePrefix"]+"forms"},
                {"Fields",DefaultSettings["DatabaseSchema"]+"."+DefaultSettings["TablePrefix"]+"fields"},
                {"Options",DefaultSettings["DatabaseSchema"]+"."+DefaultSettings["TablePrefix"]+"options "},
                {"Values",DefaultSettings["DatabaseSchema"]+"."+DefaultSettings["TablePrefix"]+"values"},
                {"Sections",DefaultSettings["DatabaseSchema"]+"."+DefaultSettings["TablePrefix"]+"sections"}
                };
            }
        }
        
        public static string DBConnectionStringWithDB
        {
            get
            {
                return "Server=" + Config.DefaultSettings["DatabaseHostname"] + ";Database=" + Config.DefaultSettings["DatabaseName"] + ";Uid=" + Config.DefaultSettings["DatabaseUsername"] + ";Pwd=" + Config.DefaultSettings["DatabasePassword"] + ";";
            }
        }

        public static string DBConnectionString
        {
            get
            {
                return "Server=" + Config.DefaultSettings["DatabaseHostname"] + ";Uid=" + Config.DefaultSettings["DatabaseUsername"] + ";Pwd=" + Config.DefaultSettings["DatabasePassword"] + ";";
            }
        }

        public Config(HttpServerUtility Server, bool isDebug = false)
        {
            try
            {
                this.isDebug = isDebug;
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
                //this.InjectWebFiles();
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
            File.Delete(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerNew.htm"));
            File.Delete(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerView.htm"));
            File.Delete(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerEdit.htm"));
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

            if (!File.Exists(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerNew.htm")))
            {
                try
                {
                    File.WriteAllText(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerNew.htm"), FormBuilderModule.Properties.Resources.DisplayerNew);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            if (!File.Exists(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerView.htm")))
            {
                try
                {
                    File.WriteAllText(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerView.htm"), FormBuilderModule.Properties.Resources.DisplayerView);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            if (!File.Exists(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerEdit.htm")))
            {
                try
                {
                    File.WriteAllText(Path.Combine(ServerContext.MapPath(templatePath), "DisplayerEdit.htm"), FormBuilderModule.Properties.Resources.DisplayerEdit);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Ensures that the required tables for the formbuilder are in place on the specified MSSQL Database.
        /// </summary>
        /// <remarks>
        /// This currently only looks for the existance of the database and tables. Need to later ensure that the columns are correct as well and update if necessary.
        /// </remarks>
        private void EnsureTableIntegrity()
        {
            using (SqlConnection conn = new SqlConnection(Config.DBConnectionString))
            {
                using (SqlCommand comm = new SqlCommand("", conn))
                {
                    //Try to open database connection;
                    try
                    {
                        conn.Open();
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }

                    if (isDebug)
                    {
                        comm.CommandText = "DROP DATABASE " + DefaultSettings["DatabaseName"];
                        try
                        {
                            comm.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        } 
                    }

                    //Check Database exists and create if not;
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM sys.databases WHERE name = @dbName)) BEGIN CREATE DATABASE " + DefaultSettings["DatabaseName"] + " END";
                    comm.Parameters.AddWithValue("@dbName", DefaultSettings["DatabaseName"]);

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
                        " BEGIN CREATE TABLE " + TableNamesSansSchema["Templates"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, Name varchar(50), Description varchar(140), CreatedDate datetime); END";
                    comm.Parameters.AddWithValue("@dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("@tableName", TableNamesSansSchema["Templates"]);
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
                        " BEGIN CREATE TABLE " + TableNamesSansSchema["Fields"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, SectionID int, Label varchar(50), Type varchar(50), Required bit, SortOrder int); END";
                    comm.Parameters.AddWithValue("dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("tableName", TableNamesSansSchema["Fields"]);
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
                        " BEGIN CREATE TABLE " + TableNamesSansSchema["Options"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, FieldID int, Value varchar(MAX), SortOrder int); END";
                    comm.Parameters.AddWithValue("dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("tableName", TableNamesSansSchema["Options"]);
                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //Check if values table exists and create if not
                    comm.Parameters.Clear();
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @dbName AND TABLE_NAME = @tableName))" +
                        " BEGIN CREATE TABLE " + TableNamesSansSchema["Values"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, FormID int, FieldID int, OptionID int, Content varchar(150)); END";
                    comm.Parameters.AddWithValue("dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("tableName", TableNamesSansSchema["Values"]);
                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //Check if sections table exists and create if not
                    comm.Parameters.Clear();
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @dbName AND TABLE_NAME = @tableName))" +
                        " BEGIN CREATE TABLE " + TableNamesSansSchema["Sections"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, FormID int, Name varchar(50), SortOrder int); END";
                    comm.Parameters.AddWithValue("dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("tableName", TableNamesSansSchema["Sections"]);
                    try
                    {
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    //Check if forms table exists and create if not
                    comm.Parameters.Clear();
                    comm.CommandText = "IF (NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_CATALOG = @dbName AND TABLE_NAME = @tableName))" +
                        " BEGIN CREATE TABLE " + TableNamesSansSchema["Forms"] + " (ID int NOT NULL IDENTITY(1,1) PRIMARY KEY, TemplateID int, CreatedDate datetime, CreatedBy varchar(50)); END";
                    comm.Parameters.AddWithValue("@dbName", DefaultSettings["DatabaseName"]);
                    comm.Parameters.AddWithValue("@tableName", TableNamesSansSchema["Forms"]);
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
