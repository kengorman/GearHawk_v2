using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using GearHawk.Core.Models;

namespace GearHawk.Core.Data
{
    public sealed class DataAccess_Node
    {
        public int DeleteNode(int nodeId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionUtils.GetConnString()))
                {
                    using (SqlCommand com = new SqlCommand("gh_Node_Delete", con))
                    {
                        com.CommandType = CommandType.StoredProcedure;
                        com.Parameters.AddWithValue("id", nodeId);
                        SqlParameter outParam = new SqlParameter("@result", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        com.Parameters.Add(outParam);
                        con.Open();
                        com.ExecuteNonQuery();
                        int iResult = (int)outParam.Value;
                        return iResult;
                    }
                }
            }
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// Important to remember, if this node instance has no categories,
        /// all categories will be cleared in the corresponding node_category table.
        /// </summary>
        /// <param name="node"></param>
        public void ProcessNodeCategories(string userId, InventoryNode node, int nodeId)
        {
            DeleteNodeCategories(userId, node);

            if (node.Categories == null || node.Categories.Count == 0)
                return;
            // note the Linq function 'Distinct()' to enforce non-duplicate categories
            foreach (Category cat in node.Categories.Distinct())
            {
                SprocUtils.ExecuteSproc("gh_Categories_AddNodeCategory",
                    new SqlParameter("userId", userId),
                    new SqlParameter("nodeId", nodeId),
                    new SqlParameter("categoryId", cat.Id),
                    new SqlParameter("globalCategories", node.GlobalCategories),
                    new SqlParameter("nodeName", node.Name));
            }
        }

        /// <summary>
        /// Anytime we update categories for a node we first delete
        /// any existing categories for that specific node.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="node"></param>
        private void DeleteNodeCategories(string userId, InventoryNode node)
        {
            if (node.id < 1)
                return;
            SprocUtils.ExecuteSproc("gh_Categories_DeleteNodeCategories",
                 new SqlParameter("userId", userId),
                  new SqlParameter("nodeId", node.id),
                  new SqlParameter("globalCategories", node.GlobalCategories),
                  new SqlParameter("nodeName", node.Name));
        }


        /// <summary>
        /// Note: 'Copying' is more complicated than 'Moving'.
        /// When we move a set of nodes, we don't change their unique ids, only their parent ids and levels.
        /// But when we copy, we insert new records for all related nodes, and also save a guid (operation code).
        /// This op code is used later when determining the source image in the Azure CDN to generate a copy.
        /// </summary>
        /// <param name="isCopy"></param>
        /// <param name="nodeToMoveId"></param>
        /// <param name="destinationNodeId"></param>
        /// <param name="userId"></param>
        /// <param name="opCode">operation code or guid</param>
        /// <returns></returns>
        public int MoveNode(bool isCopy, int nodeToMoveId, int destinationNodeId, string userId, string opCode = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionUtils.GetConnString()))
                {
                    using (SqlCommand com = new SqlCommand("gh_Node_Move", con))
                    {
                        com.CommandType = CommandType.StoredProcedure;
                        com.Parameters.AddWithValue("isCopy", isCopy);
                        com.Parameters.AddWithValue("nodeId", nodeToMoveId);
                        com.Parameters.AddWithValue("futureParentId", destinationNodeId);
                        com.Parameters.AddWithValue("userId", userId);
                        // If this is a copy, we need to save the operation code
                        // in the new rows when they are created. This allows us to find
                        // the source rows when copying images later.
                        if (isCopy)
                            com.Parameters.AddWithValue("opCode", opCode);
                        SqlParameter outputNewInventoryId = new("newInventoryId", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        com.Parameters.Add(outputNewInventoryId);
                        con.Open();
                        com.ExecuteNonQuery();
                        if (System.DBNull.Value == outputNewInventoryId.Value)
                            return -1;
                        else
                            return (int)outputNewInventoryId.Value;
                    }
                }
            }
            catch 
            {
                throw;
            }
        }

        public int AddGearCheck(InventoryNode node, string userId)
        {
            var expDate = node.ExpirationDate > (DateTime)SqlDateTime.MinValue && node.ExpirationDate < (DateTime)SqlDateTime.MaxValue ? node.ExpirationDate : null;
            SprocUtils.ExecuteSproc("gh_RigCheck_AddStatus", new SqlParameter("id", node.id),
                                                            new SqlParameter("quantity", node.Quantity),
                                                            new SqlParameter("minimumQuantity", node.MinimumQuantity),
                                                            new SqlParameter("expirationDate", expDate),
                                                            new SqlParameter("dateTime", DateTime.Now),
                                                            new SqlParameter("status", node.GearCheckStatus),
                                                            new SqlParameter("comment", node.GearCheckComment),
                                                            new SqlParameter("userId", userId),
                                                            new SqlParameter("needsRepair", node.NeedsRepair),
                                                            new SqlParameter("outOfService", node.OutOfService),
                                                            new SqlParameter("needsResupply", node.NeedsResupply));
            return 1;
        }

        public int UpdateNode(InventoryNode node)
        {
            bool isNew = node.id < 1 ? true : false;

            String sprocToExecute = isNew ? "gh_Node_Add" : "gh_Node_Update";
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionUtils.GetConnString()))
                {
                    using (SqlCommand com = new SqlCommand(sprocToExecute, con))
                    {
                        com.CommandType = CommandType.StoredProcedure;
                        if (!isNew)
                            com.Parameters.AddWithValue("id", node.id);
                        com.Parameters.AddWithValue("name", node.Name);
                        com.Parameters.AddWithValue("level", node.Level);
                        com.Parameters.AddWithValue("parentId", node.ParentId);
                        com.Parameters.AddWithValue("customerId", node.CustomerId);
                        com.Parameters.AddWithValue("customerDivision", node.CustomerDivision);
                        com.Parameters.AddWithValue("displayOrder", node.DisplayOrder);
                        com.Parameters.AddWithValue("description", node.Description);
                        com.Parameters.AddWithValue("isInventory", node.IsInventory);
                        com.Parameters.AddWithValue("includeInMaintenance", node.IncludeInMaintenance);
                        com.Parameters.AddWithValue("outOfService", node.OutOfService);
                        com.Parameters.AddWithValue("needsRepair", node.NeedsRepair);
                        com.Parameters.AddWithValue("needsResupply", node.NeedsResupply);
                        com.Parameters.AddWithValue("retiredFromService", node.RetiredFromService);
                        com.Parameters.AddWithValue("gearCheckStatus", node.GearCheckStatus);
                        com.Parameters.AddWithValue("gearCheckComment", node.GearCheckComment);
                        com.Parameters.AddWithValue("gearCheckUserId", node.GearCheckUserId);
                        com.Parameters.AddWithValue("gearCheckTime", node.GearCheckDateTime.ToLocalTime());
                        com.Parameters.AddWithValue("quantity", node.Quantity);
                        com.Parameters.AddWithValue("minimumQuantity", node.MinimumQuantity);
                        com.Parameters.AddWithValue("itemType", node.ItemType);
                        com.Parameters.AddWithValue("gearCheckInstructions", node.GearCheckInstructions);
                        if (node.ExpirationDate > (DateTime)SqlDateTime.MinValue && node.ExpirationDate < (DateTime)SqlDateTime.MaxValue)
                            com.Parameters.AddWithValue("expirationDate", node.ExpirationDate.GetValueOrDefault().ToLocalTime());
                        else
                            com.Parameters.AddWithValue("expirationDate", null);
                        com.Parameters.AddWithValue("globalDescription", node.GlobalDescription);
                        com.Parameters.AddWithValue("globalInstructions", node.GlobalInstructions);
                        com.Parameters.AddWithValue("globalCategories", node.GlobalCategories);
                        SqlParameter outParam = new SqlParameter("@nodeId", SqlDbType.Int);
                        outParam.Direction = ParameterDirection.Output;
                        com.Parameters.Add(outParam);
                        con.Open();
                        com.ExecuteNonQuery();
                        int iResult = (int)outParam.Value;
                        return iResult;
                    }
                }
            }
            catch 
            {
                throw;
            }
        }

        // In some cases we store an operation code and related value on one or more rows in the node table.
        // This permits tracking complex operations, for instance copying a small tree to another location.
        // We retrieve the relevant rows and their opValue (operation value) using the operation code.
        public static DataTable NodeOperationValue(string userId, string opCode)
        {
            return SprocUtils.ExecuteSproc("gh_OperationValue", new SqlParameter("userId", userId), new SqlParameter("opCode", opCode));
        }
    }
}
