using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class RfqModel {
        public Guid Id { get; set; }
        public int PRNumber { get; set; }
        public string VendorName { get; set; }
        public string PRType { get; set; }
        public DateTime DateRequired { get; set; }
        public string RequestedBy { get; set; }
        public string Status { get; set; }
        public bool IsQuoted { get; set; }
    }
}
