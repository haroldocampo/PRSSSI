using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PurchaseRequisitionSystem.Models {
    public class UserModel {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string DepartmentName { get; set; }
        public int UserTypeId { get; set; }
        public int AccessLevel { get; set; }
        public int CompanyId { get; set; }
        public string Email { get; set; }
    }
}
