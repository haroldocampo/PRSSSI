using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class CanvasItemModel {
        public int mItemRequestId { get; set; }
        public int ItemNumber { get; set; }
        public string ItemDescription { get; set; }
        public int Quantity { get; set; }
        public string UOM { get; set; }
        public double PriceVendor1 { get; set; }
        public double PriceVendor2 { get; set; }
        public double PriceVendor3 { get; set; }
        // Three Elemnts
        public int mCsId1 { get; set; }
        public int mCsId2 { get; set; }
        public int mCsId3 { get; set; }
        public int mRfqId1 { get; set; }
        public int mRfqId2 { get; set; }
        public int mRfqId3 { get; set; }
        public bool IsChecked1 { get; set; }
        public bool IsChecked2 { get; set; }
        public bool IsChecked3 { get; set; }
    }
}
