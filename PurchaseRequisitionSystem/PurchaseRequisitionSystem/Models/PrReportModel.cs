using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class PrReportModel {
        public string DepartmentName { get; set; }
        public string ItemDescription { get; set; }
        public int PrNumber { get; set; }
        public DateTime PrDateCreated { get; set; }
        public DateTime PrDateRequired { get; set; }
        public string PrStatus { get; set; }
    }
}
