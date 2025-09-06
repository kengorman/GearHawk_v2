using GearHawk.Core.Models;
using GearHawk.Core.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace GearHawk.Core.Processors
{
    /// <summary>
    /// TODO: need to clean up/refactor and better organize this class.
    /// Currently it is too much like bucket for several areas of the application.
    /// </summary>
    public sealed class NodeRecursion
    {
        TimeZoneInfo _easternZone;

        public NodeRecursion()
        {
            _easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        public string SpreadsheetInventoryAndProblemsJson(string userId, int inventoryId)
        {
            DataTable t = new DataAccess_EditorTree().SpreadsheetInventoryAndProblems(userId, inventoryId);
            InventoryNode parentNode = DataRowToInventoryNode(t.Rows[0]);
            foreach (DataRow row in t.Rows)
            {
                parentNode.ChildNodesExt.Add(DataRowToInventoryNode(row));
            }
            return JsonConvert.SerializeObject(parentNode);
        }

        public string NodeAndDirectDescendantsJson(int iInventoryId)
        {
            DataSet ds = DataAccess_EditorTree.NodeAndDirectDescendants("68b00d0d-a459-4e2e-82eb-18d3d7c99f80", iInventoryId);
            InventoryNode node = CreateNodeHierarchy(ds);
            return JsonConvert.SerializeObject(node);
        }

        public string BreadcrumbJson(string userId, int iInventoryId)
        {
            DataTable t = DataAccess_EditorTree.LoadInventoryBreadcrumb(userId, iInventoryId);
            return JsonConvert.SerializeObject(t);
        }

        public string TreeViewJson(string userId, int iInventoryId)
        {
            DataSet ds = DataAccess_Report.GetMaintenanceReport(iInventoryId, userId);
            DataTable dataTable = ds.Tables[0];
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["id"] ?? throw new InvalidOperationException("ID column not found") };
            DataRow dataRow = dataTable.Rows.Find(iInventoryId) ?? throw new InvalidOperationException("InventoryId not found");
            InventoryNode inventoryNode = TreeView_DataRowToInventoryNode(dataRow);
            TreeView_GetChildrenRecursive(dataTable, inventoryNode);

            return JsonConvert.SerializeObject(inventoryNode);
        }

        public string FullTreeFromRootJson(string userId)
        {
            DataSet ds = DataAccess_EditorTree.FullTreeFromRoot(userId);
            DataTable dataTable = ds.Tables[0];
            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["id"] ?? throw new InvalidOperationException("id not found") };
            DataRow dataRow = dataTable.Rows.Find((int)ds.Tables[2].Rows[0]["id"]) ?? throw new InvalidOperationException("id not found)"); ;
            InventoryNode inventoryNode = TreeView_DataRowToInventoryNode(dataRow);
            TreeView_GetChildrenRecursive(dataTable, inventoryNode);
            return JsonConvert.SerializeObject(inventoryNode);
        }

        /* ---------------------------- PRIVATE FUNCTIONS -------------------------------------------------------- */
        /* ------------------------------------------------------------------------------------------------------- */




        private InventoryNode TreeView_GetChildrenRecursive(DataTable dataTable, InventoryNode inventoryNode)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                if ((int)row["parentId"] == inventoryNode.id)
                {
                    InventoryNode childNode = TreeView_DataRowToInventoryNode(row);
                    TreeView_GetChildrenRecursive(dataTable, childNode);
                    inventoryNode.ChildNodesExt.Add(childNode);
                }
            }
            return inventoryNode;
        }

        private InventoryNode CreateNodeHierarchy(DataSet ds)
        {
            InventoryNode firstNode;
            firstNode = DataRowToInventoryNode(ds.Tables[0].Rows[0]);

            AddCategoriesToNode(firstNode, ds.Tables[1]);

            List<InventoryNode> treeNodes = [firstNode];
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                NodeHierarchyRecursive(treeNodes, row);
            }
            return firstNode;
        }

        private void AddCategoriesToNode(InventoryNode firstNode, DataTable dataTable)
        {
            if (dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    Category cat = new Category();
                    cat.Name = (string)row["name"];
                    cat.Id = (int)row["id"];
                    cat.CompanyId = (int)row["customerId"];
                    firstNode.Categories.Add(cat);
                }
            }
        }

        private InventoryNode DataRowToInventoryNode(DataRow dataRow)
        {
            InventoryNode node = new((int)dataRow["level"], (string)dataRow["name"], (int)dataRow["id"], (string)dataRow["description"])
            {
                CustomerId = (int)dataRow["customerId"],
                ParentId = (int)dataRow["parentId"]
            };
            if (dataRow.Table.Columns.Contains("categoriesString"))
                node.CategoriesString = (string)dataRow["categoriesString"];

            if (dataRow.Table.Columns.Contains("parentName") && !dataRow.IsNull("parentName"))
            {
                node.ParentName = (string)dataRow["parentName"];
            }
            node.OutOfService = (bool)dataRow["outOfService"];
            node.NeedsResupply = (bool)dataRow["needsResupply"];
            node.NeedsRepair = (bool)dataRow["needsRepair"];
            node.RetiredFromService = (bool)dataRow["retiredFromService"];
            node.IncludeInMaintenance = (bool)dataRow["includeInMaintenance"];
            node.IsInventory = (bool)dataRow["isInventory"];
            node.DisplayOrder = ((int)dataRow["displayOrder"]).ToString();
            node.CustomerCode = (string)dataRow["customerCode"];
            node.GearCheckComment = (string)dataRow["gearCheckComment"];
            node.GearCheckStatus = GetNodeStatus(dataRow);
            node.GearCheckUserId = (string)dataRow["gearCheckUserId"];
            node.GearCheckTimeWindow = (int)dataRow["gearCheckTimeWindow"];
            node.Quantity = ((int)dataRow["quantity"]).ToString();
            node.MinimumQuantity = ((int)dataRow["minimumQuantity"]).ToString();
            node.ItemType = ((int)dataRow["itemType"]).ToString();
            node.GearCheckInstructions = (string)dataRow["gearCheckInstructions"];
            if (dataRow.Table.Columns.Contains("gearCheckTime") && !dataRow.IsNull("gearCheckTime"))
            {
                var timeUtc = (DateTime)dataRow["gearCheckTime"];
                node.GearCheckDateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, _easternZone);
            }
            if (dataRow.Table.Columns.Contains("expirationDate") && !dataRow.IsNull("expirationDate"))
            {
                node.ExpirationDate = (DateTime)dataRow["expirationDate"];
            }
            return node;
        }

        /// <summary>
        /// Logic:
        /// If you don't have a valid expiration date, use the status from the datarow
        /// If the valid expiration date is != to today, mark as expired.
        /// If the valid expiration date is != to today + x days (for warning), mark as expiring
        /// Otherwise, return the status from the datarow
        /// </summary>
        /// <param name="dataRow"></param>
        /// <returns></returns>
        private string GetNodeStatus(DataRow dataRow)
        {
            //if (dataRow.Table.Columns.Contains("expirationDate") && !dataRow.IsNull("expirationDate"))
            //{
            //    var timeUtc = (DateTime)dataRow["expirationDate"];
            //    var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, _easternZone);
            //    if (convertedTime.Date <= DateTime.Now.Date)
            //        return "expired";
            //    else if (convertedTime.Date <= DateTime.Now.AddDays(14).Date)
            //        return "expiring";
            //    else
            //        return (string)dataRow["gearCheckStatus"];
            //}
            return (string)dataRow["gearCheckStatus"];
        }

        private InventoryNode TreeView_DataRowToInventoryNode(DataRow dataRow)
        {
            InventoryNode node = new((int)dataRow["level"], (string)dataRow["name"], (int)dataRow["id"])
            {
                ParentId = (int)dataRow["parentId"]
            };

            if (dataRow.Table.Columns.Contains("parentName") && !dataRow.IsNull("parentName"))
            {
                node.ParentName = (string)dataRow["parentName"];
            }
            node.OutOfService = (bool)dataRow["outOfService"];
            node.NeedsRepair = (bool)dataRow["needsRepair"];
            node.NeedsResupply = (bool)dataRow["needsResupply"];
            node.IsInventory = (bool)dataRow["isInventory"];
            node.DisplayOrder = ((int)dataRow["displayOrder"]).ToString();
            node.Level = (int)dataRow["level"];
            node.GearCheckComment = (string)dataRow["gearCheckComment"];
            node.GearCheckStatus = GetNodeStatus(dataRow);
            node.GearCheckTimeWindow = (int)dataRow["gearCheckTimeWindow"];
            node.Quantity = ((int)dataRow["quantity"]).ToString();
            node.MinimumQuantity = ((int)dataRow["minimumQuantity"]).ToString();
            node.ItemType = ((int)dataRow["itemType"]).ToString();
            node.GearCheckInstructions = (string)dataRow["gearCheckInstructions"];
            if (dataRow.Table.Columns.Contains("gearCheckTime") && !dataRow.IsNull("gearCheckTime"))
            {
                var timeUtc = (DateTime)dataRow["gearCheckTime"];
                node.GearCheckDateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, _easternZone);
            }
            if (dataRow.Table.Columns.Contains("expirationDate") && !dataRow.IsNull("expirationDate"))
            {
                node.ExpirationDate = (DateTime)dataRow["expirationDate"];
            }
            return node;
        }

        private void NodeHierarchyRecursive(List<InventoryNode> nodeCollection, DataRow row)
        {
            foreach (InventoryNode node in nodeCollection)
            {
                if (node.id == (int)row["parentId"])
                {
                    InventoryNode newNode = new InventoryNode((int)row["level"], (string)row["name"], (int)row["id"]);
                    newNode.ParentId = (int)row["parentId"];
                    newNode.Description = (string)row["description"];
                    newNode.IsInventory = (bool)row["isInventory"];
                    newNode.DisplayOrder = ((int)row["displayOrder"]).ToString();
                    newNode.NeedsRepair = (bool)row["needsRepair"];
                    newNode.NeedsResupply = (bool)row["needsResupply"];
                    newNode.OutOfService = (bool)row["outOfService"];
                    newNode.GearCheckComment = (string)row["gearCheckComment"];
                    newNode.GearCheckStatus = (string)row["gearCheckStatus"];
                    newNode.GearCheckUserId = (string)row["gearCheckUserId"];
                    newNode.GearCheckTimeWindow = (int)row["gearCheckTimeWindow"];
                    newNode.Quantity = ((int)row["quantity"]).ToString();
                    newNode.MinimumQuantity = ((int)row["minimumQuantity"]).ToString();
                    newNode.ItemType = ((int)row["itemType"]).ToString();
                    newNode.GearCheckInstructions = (string)row["gearCheckInstructions"];
                    if (row.Table.Columns.Contains("gearCheckTime") && !row.IsNull("gearCheckTime"))
                    {
                        var timeUtc = (DateTime)row["gearCheckTime"];
                        newNode.GearCheckDateTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, _easternZone);
                    }
                    if (row.Table.Columns.Contains("expirationDate") && !row.IsNull("expirationDate"))
                    {
                        newNode.ExpirationDate = (DateTime)row["expirationDate"];
                    }
                    node.ChildNodesExt.Add(newNode);
                    return;
                }
                NodeHierarchyRecursive(node.ChildNodesExt, row);
            }
        }
    }
}