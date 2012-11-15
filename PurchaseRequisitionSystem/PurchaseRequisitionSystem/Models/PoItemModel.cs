using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class PoItemModel {
        public Guid Id { get; set; }
        public int ItemNumber { get; set; }
        public int Quantity { get; set; }
        public string UOM { get; set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
    }
}
