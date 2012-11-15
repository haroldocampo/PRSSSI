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
using PurchaseRequisitionSystem.Background;
using System.Linq;
using PurchaseRequisitionSystem.Models;
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for RfqSelectRequest.xaml
	/// </summary>
	public partial class RfqSelectRequest : UserControl
	{

        private MainWindow mainWindow;
        private AsyncWork async, async2, async3;
        private PrsEntities db;

        private ViewRequest viewDialog { get; set; }
        private RfqProcessRequest rfqScreen { get; set; }
        private RequestModel model { get; set; }

		public RfqSelectRequest()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public RfqSelectRequest(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow); async3 = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            var ApprovalRequestsP = (from r in db.PurchaseRequests
                                     join ap in db.Approvals on r.PMApprovalId equals ap.Id
                                     where ap.ApprovalStatusId == 1 && r.IsQuoted == false
                                     select r); // Select only Approved
            var Requests = ApprovalRequestsP.Select(x =>
            new RequestModel
            {
                Id = x.Id,
                PRNumber = x.PRNo,
                Purpose = x.Purpose,
                PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                DateRequired = x.DateRequired,
                DateRequested = x.created_at.Value,
                Status = x.Status
            });
            dgRequests.ItemsSource = Requests;
        }

        private void btnView_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;

            viewDialog = new ViewRequest(model, mainWindow);

            viewDialog.Show();
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            rfqScreen = new RfqProcessRequest(mainWindow);

            model = (RequestModel)dgRequests.SelectedItem;

            async2.Do(PopulateItemsForProcess, PopulateItemsForProcess_Complete);
        }

        private void PopulateItemsForProcess() {
            rfqScreen.Items = new List<ItemModel>();
            IQueryable<mItemsRequest> AllItems = db.mItemsRequests;
            var itemRequests = AllItems.Where(x => x.RequestId == model.Id);
            foreach (var itemRequest in itemRequests) {
                IQueryable<Item> tmpItem = db.Items;
                var item = tmpItem.Where(x => x.Id == itemRequest.ItemId).SingleOrDefault();
                var requestItem = new ItemModel()
                {
                    ItemNumber = item.ItemNumber,
                    ext_id = itemRequest.Id,
                    Description = item.Description,
                    Brand = item.ItemBrand.Name,
                    CodeSize = item.CodeSize,
                    CostObjective = itemRequest.CostObjective.Name,
                    Quantity = itemRequest.Quantity,
                    Status = itemRequest.IsQuoteSent == true ? "Quoted" : "Not-Quoted"
                };
                rfqScreen.Items.Add(requestItem);
            }
        }

        private void PopulateItemsForProcess_Complete() {
            mainWindow.TFrame.ShowPage(rfqScreen);
        }

        private void btnMarkQuoted_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;
            async3.Do(MarkQuoted, MarkQuoted_Complete);
        }

        private void MarkQuoted() {
            var request = db.PurchaseRequests.Where(x => x.Id == model.Id).SingleOrDefault();
            request.IsQuoted = true;
            request.Status = "Request Quoted";
            db.SaveChanges();
        }

        private void MarkQuoted_Complete() {
            UserControl_Loaded(this, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new Purchasing(mainWindow));
        }

        private void btnUpdateRFQ_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select an item first.");
                return;
            }

            var requestModel = (RequestModel)dgRequests.SelectedItem;
            RfqSelectForUpdate rfqItemsDialog = new RfqSelectForUpdate(mainWindow, requestModel.Id);
            mainWindow.TFrame.ShowPage(rfqItemsDialog);
        }

        private void dgRequests_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            //if (dgRequests.SelectedIndex != -1) {
            //    btnView_Click(this, e);
            //}
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            if (txtSearch.Text.IsInteger()) {
                int searchNumber = int.Parse(txtSearch.Text);
                var Requests = db.PurchaseRequests.Where(x => x.PRNo == searchNumber).Select(x =>
                new RequestModel
                {
                    Id = x.Id,
                    PRNumber = x.PRNo,
                    Purpose = x.Purpose,
                    PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                    RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                    DateRequired = x.DateRequired,
                    DateRequested = x.created_at.Value,
                    Status = x.Status
                });

                dgRequests.ItemsSource = Requests;
            }
            else {
                MessageBox.Show("Not a valid search statement.");
            }
        }
    }
}