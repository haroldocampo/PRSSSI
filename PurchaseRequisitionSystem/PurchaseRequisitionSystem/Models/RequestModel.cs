using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace PurchaseRequisitionSystem {
    public class RequestModel {
        public Guid Id { get; set; }
        public Guid CanvasId { get; set; }
        public int PRNumber { get; set; }
        public string PRNumberModel { get; set; }
        public int StockOnHand { get; set; }
        public string Purpose { get; set; }
        public string PRType { get; set; }
        public int ItemsCount { get; set; }
        public float Price { get; set; }
        public bool IsEditable { get; set; }
        public DateTime DateRequired { get; set; }
        public DateTime DateRequested { get; set; }
        public string Status { get; set; }
        public string RequestedBy { get; set; }
    }
}
