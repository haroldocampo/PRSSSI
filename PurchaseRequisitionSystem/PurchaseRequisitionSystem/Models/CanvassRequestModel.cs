using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    class CanvassRequestModel {
        public Guid RequestId { get; set; }
        public Guid CanvasId { get; set; }
        public int PRNumber { get; set; }
        public int CSNumber { get; set; }
        public string Purpose { get; set; }
        public DateTime DateRequired { get; set; }
        public string Status { get; set; }
        public string RequestedBy { get; set; }
    }
}
