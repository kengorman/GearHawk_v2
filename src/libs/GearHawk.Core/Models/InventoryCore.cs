namespace GearHawk.Core.Models
{
    public class InventoryCore
    {
        public enum InventoryNodeAction
        {
            NO_ACTION = 0,
            ADD_NEW = 1,
            UPDATE = 2,
            DELETE = 3,
            MOVE = 4,
            STATUSCHECK = 5,
            SHOW_AUDIT = 6,
            COPY = 7
        }

        public class InventoryNode
        {
            private string? name;
            private int _id = -1;
            private int level = -1;
            private int parentId;
            // we get parentName occasionally. set to empty string
            private string parentName = "";
            private int customerId;
            public int CustomerDivision;
            private string displayOrder = "50";
            private bool isInventory = true;
            private bool includeInMaintenance = true;
            private bool outOfService = false;
            private bool needsRepair = false;
            private bool needsResupply = false;
            private bool retiredFromService = false;
            private string description = "";
            private string itemType = "1";
            private string? customerCode;
            private InventoryNodeAction action = InventoryNodeAction.NO_ACTION;
            private List<InventoryNode> _childNodesExt = new List<InventoryNode>();
            private DateTime gearCheckDateTime;
            private int gearCheckTimeWindow;
            private string? gearCheckStatus;
            private string gearCheckComment = "";
            private string gearCheckInstructions = "";
            private string gearCheckUserId = "";
            private bool gearCheckValid = false;
            private string quantity = "1";
            // default the minimum quantity to 1
            private string minimumQuantity = "1";
            private DateTime? expirationDate;
            private List<Category> categories = new List<Category>();
            // default an empty string for categoriesString
            private string categoriesString = "";
            private bool globalDescription = false;
            private bool globalInstructions = false;
            private bool globalCategories = false;

            public string Name
            {
                get
                {
                    return name;
                }

                set => name = value;
            }

            public int id
            {
                get
                {
                    return _id;
                }

                set
                {
                    _id = value;
                }
            }

            public int Level
            {
                get
                {
                    return level;
                }

                set
                {
                    level = value;
                }
            }

            public int ParentId
            {
                get
                {
                    return parentId;
                }

                set
                {
                    parentId = value;
                }
            }
            public int CustomerId
            {
                get
                {
                    return customerId;
                }

                set
                {
                    customerId = value;
                }
            }
            public string DisplayOrder
            {
                get
                {
                    return displayOrder;
                }

                set
                {
                    displayOrder = value;
                }
            }

            public bool IsInventory
            {
                get
                {
                    return isInventory;
                }

                set
                {
                    isInventory = value;
                }
            }

            public bool IncludeInMaintenance
            {
                get
                {
                    return includeInMaintenance;
                }

                set
                {
                    includeInMaintenance = value;
                }
            }

            public bool OutOfService
            {
                get
                {
                    return outOfService;
                }

                set
                {
                    outOfService = value;
                }
            }

            public bool NeedsRepair
            {
                get
                {
                    return needsRepair;
                }

                set
                {
                    needsRepair = value;
                }
            }

            public bool RetiredFromService
            {
                get
                {
                    return retiredFromService;
                }

                set
                {
                    retiredFromService = value;
                }
            }

            public string Description
            {
                get
                {
                    return description;
                }

                set
                {
                    description = value;
                }
            }

            public List<InventoryNode> ChildNodesExt { get => _childNodesExt; set => _childNodesExt = value; }
            public string CustomerCode { get => customerCode; set => customerCode = value; }
            public InventoryNodeAction Action { get => action; set => action = value; }
            public string ParentName { get => parentName; set => parentName = value; }
            public DateTime GearCheckDateTime
            {
                get { return gearCheckDateTime; }
                set
                {
                    gearCheckDateTime = value;
                }
            }
            public string GearCheckStatus { get => gearCheckStatus; set => gearCheckStatus = value; }
            public string GearCheckComment { get => gearCheckComment; set => gearCheckComment = value; }
            public string GearCheckUserId { get => gearCheckUserId; set => gearCheckUserId = value; }
            public bool GearCheckValid
            {
                get
                {
                    return gearCheckValid;
                }
                set
                {
                    gearCheckValid = value;

                }
            }
            public int GearCheckTimeWindow
            {
                get
                {
                    return gearCheckTimeWindow;
                }
                set
                {
                    gearCheckTimeWindow = value;
                    if (gearCheckTimeWindow <= 4)
                        GearCheckValid = true;
                    else
                        GearCheckValid = false;
                }
            }

            public string Quantity { get => quantity; set => quantity = value; }
            public DateTime? ExpirationDate { get => expirationDate; set => expirationDate = value; }
            public bool NeedsResupply { get => needsResupply; set => needsResupply = value; }
            public string GearCheckInstructions { get => gearCheckInstructions; set => gearCheckInstructions = value; }
            public string ItemType { get => itemType; set => itemType = value; }
            public string MinimumQuantity { get => minimumQuantity; set => minimumQuantity = value; }
            public List<Category> Categories { get => categories; set => categories = value; }
            public string CategoriesString { get => categoriesString; set => categoriesString = value; }
            public bool GlobalDescription { get => globalDescription; set => globalDescription = value; }
            public bool GlobalInstructions { get => globalInstructions; set => globalInstructions = value; }
            public bool GlobalCategories { get => globalCategories; set => globalCategories = value; }

            public InventoryNode() { }
            public InventoryNode(int level) : this(level, null) { }
            public InventoryNode(int level, string name) : this(level, name, -1) { }
            public InventoryNode(int level, string name, int iD) : this(level, name, iD, "") { }
            public InventoryNode(int level, string? name, int iD, string description)
            {

                Level = level;
                Name = name ?? throw new ArgumentException(null, nameof(name));
                id = iD;
                Description = description;
            }
        }

    }
}
