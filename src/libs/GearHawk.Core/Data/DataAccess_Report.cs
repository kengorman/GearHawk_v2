
using System;
using System.Data;
using Microsoft.Data.SqlClient;


namespace GearHawk.Core.Data
{
    public static class DataAccess_Report
    {
        public static DataSet GetMaintenanceReport(int id, string userId)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionUtils.GetConnString()))
                {
                    using (SqlCommand com = new SqlCommand("gh_Report_Maintenance", con))
                    {
                        com.CommandType = CommandType.StoredProcedure;
                        com.Parameters.AddWithValue("id", id);
                        com.Parameters.AddWithValue("userId", userId);

                        SqlDataAdapter da = new()
                        {
                            SelectCommand = com
                        };
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


        public static DataSet GetOosReport(string userId)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionUtils.GetConnString()))
                {
                    using (SqlCommand com = new SqlCommand("gh_OosReport", con))
                    {
                        com.CommandType = CommandType.StoredProcedure;
                        com.Parameters.AddWithValue("userId", userId);

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
