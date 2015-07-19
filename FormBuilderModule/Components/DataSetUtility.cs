using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Beefry
{
    public static class DataSetExension
    {
        private static bool HasResults(this DataSet ds)
        {
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static DataRowCollection GetResults(this DataSet ds)
        {
            if (ds.HasResults())
            {
                return ds.Tables[0].Rows;
            }
            else
            {
                return null;
            }
        }
    }
}
