using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class PrCompletedReportModels {
        public string DepartmentName { get; set; }
        public string CostObjective { get; set; }
        public string ItemDescription { get; set; }
        public int PrNumber { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string UOM { get; set; }
        public double TotalAmount { get; set; }
        public string Purpose { get; set; }
        public DateTime PrDateCreated { get; set; }
        public DateTime PrDateRequired { get; set; }
        public string PrStatus { get; set; }
    }
}
