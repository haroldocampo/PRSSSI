using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PurchaseRequisitionSystem.Models {
    public class ItemReceivingModel {
        public int Id { get; set; }
        public string Description { get; set; }
        public string UOM { get; set; }
        public int QuantityRequested { get; set; }
        public string QuantityReceived { get; set; }
        public double ItemPrice { get; set; }
        public double DateCreated { get; set; }
        public string DrInvoiceNumber { get; set; }
    }
}
