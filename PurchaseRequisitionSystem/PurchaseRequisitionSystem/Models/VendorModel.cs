using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class VendorModel {
        public int Id { get; set; }
        public string VendorName { get; set; }
        public string TIN { get; set; }
        public string Terms { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
        public string FaxNumber { get; set; }

    }
}
