using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class PoListModel {
        public Guid Id { get; set; }
        public int PONumber { get; set; }
        public string Vendor { get; set; }
        public string VendorAddress { get; set; }
        public int VendorId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string RequestedBy { get; set; }
        public DateTime DateRequired { get; set; }
        public double GrandTotal { get; set; }
    }
}
