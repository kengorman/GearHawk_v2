
using GearHawk.Core.Data;
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace GearHawk.Core.Data
{
    public sealed class DataAccess_EditorTree
    {
        public DataTable SpreadsheetInventoryAndProblems(string userId, int inventoryId)
        {
            return SprocUtils.ExecuteSproc("gh_Report_InventoryAndProblems", new SqlParameter("id", inventoryId), new SqlParameter("userId", userId));
        }

        public static DataTable LoadInventoryBreadcrumb(string userId, int inventoryId)
        {
            return SprocUtils.ExecuteSproc("gh_Node_Breadcrumb", new SqlParameter("@nodeId", inventoryId), new SqlParameter("@userId", userId));
        }

        public static DataSet NodeAndDirectDescendants(string sUserId, int iInventoryId)
        {
            return SprocUtils.ExecuteSproc_DataSet("gh_Node_AndDirectDescendants", new SqlParameter("id", iInventoryId), new SqlParameter("userId", sUserId));
        }

        public static DataSet FullTreeFromRoot(string sUserId)
        {
            return SprocUtils.ExecuteSproc_DataSet("gh_FullTreeFromRoot", new SqlParameter("userId", sUserId));
        }
    }
}
