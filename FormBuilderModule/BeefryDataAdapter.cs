using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Beefry.FormBuilder
{
    public abstract class BeefryDataAdapter
    {
        protected SqlConnection conn;
        protected SqlCommand comm;
        protected SqlDataAdapter da;
        protected DataSet ds;

        /// <summary>
        /// Constructor for the FormDataAdapter. Initiates all communication to the database and all data aquisition.
        /// </summary>
        /// <param name="refForm">the Form that is used to make CRUD actions on. SHOULD be passed by reference by default.</param>
        public BeefryDataAdapter()
        {
            conn = new SqlConnection(Config.DBConnectionStringWithDB);
            comm = new SqlCommand();
            comm.Connection = conn;
        }

        protected void ClearData()
        {
            ds.Clear();
            comm.Parameters.Clear();
        }
    }
}
