using System.Collections.Generic;

namespace ImportApp.Models
{
    public class ImportViewModel
    {
        public List<string> Headers { get; set; }
        public Dictionary<string, List<string>> ColumnValues { get; set; }
        public List<ColumnMapping> ColumnMappings { get; set; }
        public int StartingRow { get; set; }
        public bool IsFileUploaded { get; set; }

        public static List<string> StaticColumns { get; } = new List<string>
        {
            "Pickup store #",
            "Pickup store Name",
            "Pickup lat",
            "Pickup lon",
            "Pickup formatted Address",
            "Pickup Contact Name First Name",
            "Pickup Contact Name Last Name",
            "Pickup Contact Email",
            "Pickup Contact Mobile Number",
            "Pickup Enable SMS Notification",
            "Pickup Time",
            "Pickup tolerance (min)",
            "Pickup Service Time",
            "Delivery store #",
            "Delivery store Name",
            "Delivery lat (req if adding new customer)",
            "Delivery long (req if adding new customer)",
            "Delivery formatted Address",
            "Delivery Contact First Name",
            "Delivery Contact Last Name",
            "Delivery Contact Email",
            "Delivery Contact Mobile Number (need 0 at the front)",
            "Delivery Enable SMS Notification (No=0/Yes=1)",
            "Delivery Time",
            "Delivery Tolerance (Min past Delivery Time)",
            "Delivery Service Time (min)",
            "Order Details",
            "Assigned Driver",
            "Customer reference",
            "Payer",
            "Vehicle",
            "Weight",
            "Price"
        };
    }

    public class ColumnMapping
    {
        public string StaticName { get; set; }
        public string SelectedColumn { get; set; }
    }
}
