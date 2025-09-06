using System;
using System.Data;
using GearHawk.Core.Data;
using GearHawk.Core.Models;

namespace GearHawk.Core.Processors
{
    public enum DeleteNodeResult
    {
        DELETE_ALL_CHILD_NODES_FIRST = -901,
        LEVEL_ONE_NODES_MAY_NOT_BE_DELETED = -902,
        NODE_DOES_NOT_EXIST = -903,
        DELETE_SUCCESSFUL = -999,
        UNKNOWN = -1000
    }

    public sealed class NodeDataProcessor
    {
        public int UpdateNode(string userId, InventoryNode node)
        {
            int newNodeId = -1;
            DataAccess_Node nodeUpdater = new DataAccess_Node();
            newNodeId = nodeUpdater.UpdateNode(node);
            nodeUpdater.ProcessNodeCategories(userId, node, newNodeId);
            return newNodeId;
        }

        public DeleteNodeResult DeleteNode(int nodeId)
        {
            DataAccess_Node nodeDeleter = new DataAccess_Node();
            return (DeleteNodeResult)nodeDeleter.DeleteNode(nodeId);
        }

        public int MoveNode(int nodeToMoveId, int destinationNodeId, string userId)
        {
            DataAccess_Node nodeMover = new DataAccess_Node();

            bool isCopy = false;
            return nodeMover.MoveNode(isCopy, nodeToMoveId, destinationNodeId, userId);
        }

        /// <summary>
        /// The copy node operation requires an additional field called opCode (operation code)
        /// This code is save in each newly inserted record, as well as the original id of the record being copied.
        /// (See opCode and opVal in the node table.)
        /// </summary>
        /// <param name="nodeToCopyId"></param>
        /// <param name="destinationNodeId"></param>
        /// <param name="userId"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public int CopyNode(int nodeToCopyId, int destinationNodeId, string userId, string opCode)
        {
            DataAccess_Node nodeMover = new DataAccess_Node();
            bool isCopy = true;
            return nodeMover.MoveNode(isCopy, nodeToCopyId, destinationNodeId, userId, opCode);
        }

        public int AddGearCheck(InventoryNode node, string userId)
        {
            return new DataAccess_Node().AddGearCheck(node, userId);
        }

        /// <summary>
        /// Returns all the records in the node table with this opCode.
        /// We use the related opValue field to perform follow-up operations on these rows.
        /// For instance, when copying node rows, we later need to copy the related images
        /// in the Azure CDN. The operation value field points to the original id so we can find and copy the image.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public DataTable NodeOperationValue(string userId, string opCode)
        {
            return DataAccess_Node.NodeOperationValue(userId, opCode);
        }
    }
}
