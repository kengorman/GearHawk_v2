using GearHawk.Core.Data;
using System.Data;
using Microsoft.Data.SqlClient;

namespace GearHawk.Core.Data
{
    public sealed class DataAccess_RigCheck
    {
        public static DataTable LoadAuditHistory(string userId, int nodeId)
        {
            return SprocUtils.ExecuteSproc("gh_RigCheck_AuditHistory",
                        new SqlParameter("userId", userId),
                        new SqlParameter("nodeId", nodeId));
        }
        public static DataTable PercentComplete(string userId, int nodeId)
        {
            return SprocUtils.ExecuteSproc("gh_RigCheck_PctComplete",
                        new SqlParameter("userId", userId),
                        new SqlParameter("nodeId", nodeId));
        }
    }
};