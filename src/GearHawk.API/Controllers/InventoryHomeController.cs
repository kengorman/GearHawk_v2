using GearHawk.Core.Helpers;
using GearHawk.Core.Models;
using GearHawk.Core.Processors;
using GearHawk.Core.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;


namespace GearHawk.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryHomeController : ControllerBase
    {
        public InventoryHomeController()
        {
        }

        private async Task<bool> IsInRole(string userId, string roleName)
        {
            //var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("user not found");
            //var bIsInRole = await _userManager.IsInRoleAsync(user, roleName);
            //if (!bIsInRole)
            //    return false;
            await Task.Delay(100);
            return true;
        }

        [HttpPost]
        public async Task<string> Post(InventoryNode node, int i)
        {
            string userId = UserHelper.UserId(this.User) ?? throw new InvalidOperationException("UserId not found");
            node.GearCheckUserId = userId;
            try
            {
                switch (node.Action)
                {
                    case InventoryNodeAction.NO_ACTION:
                        break;
                    case InventoryNodeAction.ADD_NEW:
                    case InventoryNodeAction.UPDATE:
                        bool bCanEdit = true;// await IsInRole(userId, "Edit");
                        if (!bCanEdit)
                        {
                            return "denied";
                        }
                        int inventoryId = new NodeDataProcessor().UpdateNode(userId, node);
                        if (node.id == -2)
                            UseDefaultImage(node.CustomerCode, inventoryId);
                        return "ok";
                    case InventoryNodeAction.MOVE:
                        bool bCanMove = true; // await IsInRole(userId, "MoveCopy");
                        if (!bCanMove)
                        {
                            return "denied";
                        }
                        new NodeDataProcessor().MoveNode(node.id, node.ParentId, userId);
                        return "ok";
                    case InventoryNodeAction.COPY:
                        bool bCanCopy = true; // await IsInRole(userId, "MoveCopy");
                        if (!bCanCopy)
                        {
                            return "denied";
                        }
                        // Unlike when rows are moved, when they are copied
                        // we use an operation code that is inserted in each new row.
                        // This is used later to retrieve the original image from the CDN in order to copy it.
                        string opCode = Guid.NewGuid().ToString();
                        NodeDataProcessor nodeProcessor = new();
                        nodeProcessor.CopyNode(node.id, node.ParentId, userId, opCode);
                        DataTable opTable = nodeProcessor.NodeOperationValue(userId, opCode);
                        if (opTable != null && opTable.Rows.Count != 0)
                        {
                            foreach (DataRow row in opTable.Rows)
                            {
                                StorageHelper.InventoryBlobCopy((int)row["opVal"], (int)row["id"]);
                            }
                        }
                        return "ok";
                    case InventoryNodeAction.DELETE:
                        bool bCanDelete = await IsInRole(userId, "Delete");
                        if (!bCanDelete)
                        {
                            return "denied";
                        }
                        new NodeDataProcessor().DeleteNode(node.id);
                        return "ok";
                    case InventoryNodeAction.STATUSCHECK:
                        new NodeDataProcessor().AddGearCheck(node, userId);
                        return "ok";
                    default:
                        return "denied";
                }
            }
            catch (Exception ex)
            {
                return "error while saving: " + ex.Message;
            }
            return "ok";
        }

        [HttpGet]
        public IActionResult Get(int? id, int? action)
        {
            string? uid = User?.Claims?.FirstOrDefault(c => c.Type == "user_id")?.Value;
            List<string> results = InventoryJsonAndUserRoles(id);
            string jsonString = JsonConvert.SerializeObject(results);
            if (action == null || action == 0)
            {
                return Ok(new { message = jsonString, userId = uid });
            }
            else
            {
                return (IActionResult)Audit(id);
            }
        }

        /************************************* PRIVATE FUNCTIONS *******************************************/

        private void UseDefaultImage(string customerCode, int inventoryId)
        {
            try
            {
                StorageHelper.CopyDefaultImage(customerCode, inventoryId);
            }
            catch
            {
                throw;
            }
        }

        private List<string> InventoryJsonAndUserRoles(int? nodeId)
        {
             List<string> list = NodeAndDirectDescendants("68b00d0d-a459-4e2e-82eb-18d3d7c99f80",nodeId);
          //  list.Add(Breadcrumb("22", nodeId));
  //          var roles = UserRoles(userId);
           // list.Add("test");// roles.Result);
            return list;
        }

        private static string UserRoles(string userId)
        {
            //var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException($"User with ID {userId} not found"); ;
            //var roles = await _userManager.GetRolesAsync(user);
            //return string.Join(",", roles.ToArray()); ;
            return string.Join(",", ["deprecated, deprecated"]);
        }

        private string Breadcrumb(string userId, int? id)
        {
            return new NodeRecursion().BreadcrumbJson(userId, id ?? -1);
        }

        private List<string> NodeAndDirectDescendants(string userId, int? id)
        {
            List<string> list = [new NodeRecursion().NodeAndDirectDescendantsJson(id ?? -1)];
            return list;
        }

        private List<string> Audit(int? id)
        {
            if (User == null) throw new InvalidOperationException("User is null");
            string? userId = UserHelper.UserId(User);
            if (userId == null)
            {
                var errorList = new List<string>
                {
                    "error... no user...",
                    "error... no user for breadcrumb..."
                };
                return errorList;
            }
            List<string> list = [new RigCheck().RigCheckHistory(userId, id ?? -1)];
            return list;
        }
    }
}