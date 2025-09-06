using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace GearHawk.Core.Data
{
    class SprocUtils
    {
        public static DataTable ExecuteSproc(string sSproName, params SqlParameter[] arrParams)
        {
            DataTable table = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionUtils.GetConnString()))
                {
                    using (SqlCommand com = new SqlCommand(sSproName, con))
                    {
                        com.CommandType = CommandType.StoredProcedure;
                        if (arrParams != null && arrParams.Length > 0)
                            com.Parameters.AddRange(arrParams);
                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = com;
                        da.Fill(table);
                    }
                }
                return table;
            }
            catch 
            {
                throw;
            }
        }

        public static DataSet ExecuteSproc_DataSet(string sSproName, params SqlParameter[] arrParams)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionUtils.GetConnString()))
                {
                    using (SqlCommand com = new SqlCommand(sSproName, con))
                    {
                        com.CommandType = CommandType.StoredProcedure;
                        if (arrParams != null && arrParams.Length > 0)
                            com.Parameters.AddRange(arrParams);
                        SqlDataAdapter da = new SqlDataAdapter();
                        da.SelectCommand = com;
                        da.Fill(ds);
                    }
                }
                return ds;
            }
            catch
            {
                throw;
            }
        }
    }
}
