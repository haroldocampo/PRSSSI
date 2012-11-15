using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class CompanyModel {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
