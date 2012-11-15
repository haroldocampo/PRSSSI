using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections;
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Enums;
using PurchaseRequisitionSystem.Background;
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for OverallHeadApproval.xaml
	/// </summary>
	public partial class ApprovalRequest : UserControl
	{
        private PrsEntities db;
        private MainWindow mainWindow;
        private Guid userId;
        private UserModel User;
        private RequestModel model;
        private AsyncWork async, async2;
        private IEnumerable Requests;
        private ViewRequest viewDialog;
        int searchNumber;


		public ApprovalRequest()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public ApprovalRequest(MainWindow mainWindow) :this() {
            this.mainWindow = mainWindow;
            User = (UserModel)App.Current.Properties["User"];
            userId = User.Id;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            LoadFromUserType();
            dgRequests.ItemsSource = Requests;
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;
            async.Do(ProcessRequest, ApprovalStatuses.Approved, TransactionComplete);
        }

        private void btnSeeMe_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;
            async.Do(ProcessRequest, ApprovalStatuses.PleaseSeeMe, TransactionComplete);
        }

        private void btnDisapprove_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;
            async.Do(ProcessRequest, ApprovalStatuses.Disapproved, TransactionComplete);
        }

        private void TransactionComplete() {
            MessageBox.Show("Operation Complete");
            mainWindow.TFrame.ShowPage(new ApprovalRequest(mainWindow));
        }

        private void ProcessRequest(ApprovalStatuses Status) {
            var Request = db.PurchaseRequests.Where(x => x.Id == model.Id).SingleOrDefault();

            Guid approvalId = Guid.NewGuid();

            //Turn Enum into integer value
            int StatusId = (int)Status;

            //Set Approval
            Approval NewApproval = new Approval();
            NewApproval.Id = approvalId;
            NewApproval.ApprovalStatusId = StatusId;
            NewApproval.ApprovalUser = User.Id;
            db.AddToApprovals(NewApproval);

            string VarStatus = db.ApprovalStatuses.Where(x => x.Id == StatusId).SingleOrDefault().Status;

            switch (User.UserTypeId) {
                case (int)UserTypes.OverallHead:
                    Request.OHApprovalId = approvalId;
                    Request.Status = "{0} By Overall Head".WithTokens(VarStatus);
                    break;
                case (int)UserTypes.PlantManager:
                    Request.PMHApprovalId = approvalId;
                    Request.Status = "{0} By Plant Mngr./Dept. Head".WithTokens(VarStatus);
                    if (StatusId == 1) { //IF approved by plant manager
                        Request.IsEditable = false; // Disable editing
                    }
                    break;
                case (int)UserTypes.Purchasing:
                    Request.PMApprovalId = approvalId;
                    Request.Status = "{0} By Purchasing".WithTokens(VarStatus);
                    break;
                case (int)UserTypes.VicePresident:
                    Request.VPApprovalId = approvalId;
                    Request.Status = "{0} By Vice President".WithTokens(VarStatus);
                    break;
            }
            db.SaveChanges();
        }

        private void btnView_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;

            viewDialog = new ViewRequest(model, mainWindow);

            viewDialog.ShowDialog();
        }

        private void LoadFromUserType() {

            switch (User.UserTypeId) {
                case (int)UserTypes.PlantManager:
                    var ApprovalRequestsPMH = (from r in db.PurchaseRequests
                               join u in db.Users on r.RequestedBy equals u.Id
                               join ap in db.Approvals on r.PMHApprovalId equals ap.Id into JoinedAppReq
                               from ra in JoinedAppReq.DefaultIfEmpty()
                               where (ra.ApprovalStatusId == 3 || r.PMHApprovalId == null) && u.CompanyId == User.CompanyId
                               select r); // Select only Approved or see me
                    Requests = ApprovalRequestsPMH.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber = x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
                case (int)UserTypes.OverallHead:
                    var ApprovalRequestsOH = (from r in db.PurchaseRequests
                                              join apthis in db.Approvals on r.OHApprovalId equals apthis.Id into joined
                                              join u in db.Users on r.RequestedBy equals u.Id
                                              from ra in joined.DefaultIfEmpty()
                                              join ap in db.Approvals on r.PMHApprovalId equals ap.Id
                                              where ap.ApprovalStatusId == 1 && (r.OHApprovalId == null || ra.ApprovalStatusId == 3) && u.CompanyId == User.CompanyId
                                              select r); // Select only Approved
                    Requests = ApprovalRequestsOH.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber = x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
                case (int)UserTypes.Purchasing:
                    var ApprovalRequestsP = (from r in db.PurchaseRequests
                                              join apthis in db.Approvals on r.PMApprovalId equals apthis.Id into joined
                                              from ra in joined.DefaultIfEmpty()
                                              join ap in db.Approvals on r.OHApprovalId equals ap.Id
                                              where ap.ApprovalStatusId == 1 && (r.PMApprovalId == null || ra.ApprovalStatusId == 3)
                                              select r); // Select only Approved
                    Requests = ApprovalRequestsP.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber = x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
                case (int)UserTypes.VicePresident:
                    var ApprovalRequestsVP = (from r in db.PurchaseRequests
                                              join apthis in db.Approvals on r.VPApprovalId equals apthis.Id into joined
                                              from ra in joined.DefaultIfEmpty()
                                              join ap in db.Approvals on r.PMApprovalId equals ap.Id
                                              where ap.ApprovalStatusId == 1 && (r.VPApprovalId == null || ra.ApprovalStatusId == 3)
                                              select r); // Select only Approved
                    Requests = ApprovalRequestsVP.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber = x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
            }
        }

        private void SearchFromUserType() {

            switch (User.UserTypeId) {
                case (int)UserTypes.PlantManager:
                    var ApprovalRequestsPMH = (from r in db.PurchaseRequests
                                               join u in db.Users on r.RequestedBy equals u.Id
                                               join ap in db.Approvals on r.PMHApprovalId equals ap.Id into JoinedAppReq
                                               from ra in JoinedAppReq.DefaultIfEmpty()
                                               where (ra.ApprovalStatusId == 3 || r.PMHApprovalId == null) && r.PRNo == searchNumber && u.CompanyId == User.CompanyId
                                               select r); // Select only Approved or see me
                    Requests = ApprovalRequestsPMH.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber = x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
                case (int)UserTypes.OverallHead:
                    var ApprovalRequestsOH = (from r in db.PurchaseRequests
                                              join u in db.Users on r.RequestedBy equals u.Id
                                              join apthis in db.Approvals on r.OHApprovalId equals apthis.Id into joined
                                              from ra in joined.DefaultIfEmpty()
                                              join ap in db.Approvals on r.PMHApprovalId equals ap.Id
                                              where ap.ApprovalStatusId == 1 && (r.OHApprovalId == null || ra.ApprovalStatusId == 3)
                                              && r.PRNo == searchNumber && u.CompanyId == User.CompanyId
                                              select r); // Select only Approved
                    Requests = ApprovalRequestsOH.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber = x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
                case (int)UserTypes.Purchasing:
                    var ApprovalRequestsP = (from r in db.PurchaseRequests
                                             join apthis in db.Approvals on r.PMApprovalId equals apthis.Id into joined
                                             from ra in joined.DefaultIfEmpty()
                                             join ap in db.Approvals on r.OHApprovalId equals ap.Id
                                             where ap.ApprovalStatusId == 1 && (r.PMApprovalId == null || ra.ApprovalStatusId == 3)
                                             && r.PRNo == searchNumber
                                             select r); // Select only Approved
                    Requests = ApprovalRequestsP.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber = x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
                case (int)UserTypes.VicePresident:
                    var ApprovalRequestsVP = (from r in db.PurchaseRequests
                                              join apthis in db.Approvals on r.VPApprovalId equals apthis.Id into joined
                                              from ra in joined.DefaultIfEmpty()
                                              join ap in db.Approvals on r.PMApprovalId equals ap.Id
                                              where ap.ApprovalStatusId == 1 && (r.VPApprovalId == null || ra.ApprovalStatusId == 3)
                                              && r.PRNo == searchNumber
                                              select r); // Select only Approved
                    Requests = ApprovalRequestsVP.Select(x =>
                    new RequestModel
                    {
                        Id = x.Id,
                        PRNumber =  x.PRNo,
                        PRNumberModel = x.PRNoModel,
                        Purpose = x.Purpose,
                        PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                        RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                        DateRequired = x.DateRequired,
                        DateRequested = x.created_at.Value,
                        Status = x.Status
                    });
                    break;
            }

            dgRequests.ItemsSource = Requests;
        }

        private void dgRequests_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (dgRequests.SelectedIndex != -1) {
                btnView_Click(this, e);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            if (txtSearch.Text.IsInteger()) {
                searchNumber = txtSearch.Text.ToInt();
                SearchFromUserType();
            }
            else {
                MessageBox.Show("Not a valid search statement.");
            }
        }
	}
}