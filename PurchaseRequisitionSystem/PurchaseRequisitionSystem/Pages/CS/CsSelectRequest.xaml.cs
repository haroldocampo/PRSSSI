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
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Background;
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for CsSelectRequest.xaml
	/// </summary>
	public partial class CsSelectRequest : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;
        private RequestModel model;
        private AsyncWork async,asyncLoad,asyncMark;
        private IQueryable<RequestModel> Requests;

		public CsSelectRequest()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public CsSelectRequest(MainWindow mainWindow) :this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow); asyncLoad = new AsyncWork(mainWindow); asyncMark = new AsyncWork(mainWindow); 
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            asyncLoad.Do(PopulateRequests, PopulateRequests_Complete);
        }

        private void PopulateRequests_Complete() {
            dgRequests.ItemsSource = Requests;
        }

        private void PopulateRequests() {
            var ApprovalRequestsP = (from r in db.PurchaseRequests
                                     where r.IsQuoted == true && r.IsCanvased == false
                                     select r); // Select only Approved
            Requests = ApprovalRequestsP.Select(x =>
            new RequestModel
            {
                Id = x.Id,
                PRNumberModel = x.PRNoModel,
                Purpose = x.Purpose,
                PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                DateRequired = x.DateRequired,
                DateRequested = x.created_at.Value,
                Status = x.Status
            });
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            //Validation
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select an item first.");
                return;
            }

            //Instantiation
            CsItemSelect csItemSelectDialog = new CsItemSelect(mainWindow);

            //Select Request Model
            model = (RequestModel)dgRequests.SelectedItem;
            csItemSelectDialog.RequestId = model.Id;
            // Show Page
            mainWindow.TFrame.ShowPage(csItemSelectDialog);
        }

        private void btnView_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;

            ViewRequest viewDialog = new ViewRequest(model, mainWindow);

            viewDialog.Show();
        }

        private void dgRequests_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            //if (dgRequests.SelectedIndex != -1) {
            //    btnView_Click(this, e);
            //}
        }

        private void btnMarkCanvassed_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;

            async.Do(MarkFinished, MarkFinished_Complete);
        }

        private void MarkFinished() {
            var request = db.PurchaseRequests.Where(x => x.Id == model.Id).SingleOrDefault();
            request.IsCanvased = true;
            db.SaveChanges();
        }

        private void MarkFinished_Complete() {
            mainWindow.TFrame.ShowPage(new CsSelectRequest(mainWindow));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            if (txtSearch.Text.IsInteger()) {
                int searchNumber = int.Parse(txtSearch.Text);
                var Requests = db.PurchaseRequests.Where(x => x.PRNo == searchNumber).Select(x =>
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

                dgRequests.ItemsSource = Requests;
            }
            else {
                MessageBox.Show("Not a valid search statement.");
            }
        }

    }
}