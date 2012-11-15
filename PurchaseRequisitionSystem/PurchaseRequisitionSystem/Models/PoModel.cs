using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class PoModel {
        public Guid Id { get; set; }
        public int PoNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DeliveryDate { get; set; }
        public List<RfqItemModel> items { get; set; }
        public int VendorId { get; set; }
    }
}
