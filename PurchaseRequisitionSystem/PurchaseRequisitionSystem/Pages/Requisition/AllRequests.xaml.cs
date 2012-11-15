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

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for AllRequests.xaml
	/// </summary>
	public partial class AllRequests : UserControl
	{
        private PrsEntities db;
        private AsyncWork asyncLoad;
        private IEnumerable<RequestModel> Requests;
        private MainWindow mainWindow;

		public AllRequests()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public AllRequests(MainWindow mainWindow) :this() {
            this.mainWindow = mainWindow;
            asyncLoad = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            asyncLoad.Do(PopulateRequests, PopulateRequests_Complete);
        }

        private void PopulateRequests_Complete() {
            dgRequests.ItemsSource = Requests;
        }

        private void PopulateRequests() {
            var ApprovalRequestsP = (from r in db.PurchaseRequests
                                     join apthis in db.Approvals on r.PMHApprovalId equals apthis.Id into joined
                                     from ra in joined.DefaultIfEmpty()
                                     select new { r, ra }); // Select only Approved
            Requests = ApprovalRequestsP.Select(x =>
            new RequestModel
            {
                Id = x.r.Id,
                PRNumber = x.r.PRNo,
                PRNumberModel = x.r.PRNoModel,
                Purpose = x.r.Purpose,
                PRType = x.r.PRType_IsStock == true ? "Stock" : "Non-Stock",
                RequestedBy = x.r.User.LastName + ", " + x.r.User.FirstName,
                DateRequired = x.r.DateRequired,
                DateRequested = x.r.created_at.Value,
                Status = x.r.Status
            });
        }

        private void listItem_Loaded(object sender, RoutedEventArgs e) {
            StackPanel item = sender as StackPanel;
            var model = item.DataContext as RequestModel;

            if (model.Status.StartsWith("Consult Me")) {
                item.Background = new SolidColorBrush(Color.FromArgb(200, 230, 240, 0));
                return;
            }

            if (model.Status.StartsWith("Disapproved")) {
                item.Background = new SolidColorBrush(Color.FromArgb(200, 200, 0, 0));
                return;
            }

            if (model.Status.StartsWith("Approved")) {
                item.Background = new SolidColorBrush(Color.FromArgb(200, 0, 200, 0));
                return;
            }
        }

        private void btnView_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Select a request first");
                return;
            }

            RequestModel model = dgRequests.SelectedItem as RequestModel;
            ViewRequest request = new ViewRequest(model, mainWindow);
            request.ShowDialog();
        }

        private void dgRequests_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (dgRequests.SelectedIndex != -1) {
                btnView_Click(this, e);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            int searchNumber = int.Parse(txtSearch.Text);
            var ApprovalRequestsP = (from r in db.PurchaseRequests
                                     join apthis in db.Approvals on r.PMHApprovalId equals apthis.Id into joined
                                     from ra in joined.DefaultIfEmpty()
                                     where r.PRNo == searchNumber
                                     select new { r, ra }); // Select only Approved
            var Requests = ApprovalRequestsP.Select(x =>
            new RequestModel
            {
                Id = x.r.Id,
                PRNumber = x.r.PRNo,
                PRNumberModel = x.r.PRNoModel,
                Purpose = x.r.Purpose,
                PRType = x.r.PRType_IsStock == true ? "Stock" : "Non-Stock",
                RequestedBy = x.r.User.LastName + ", " + x.r.User.FirstName,
                DateRequired = x.r.DateRequired,
                DateRequested = x.r.created_at.Value,
                Status = x.r.Status
            });

            dgRequests.ItemsSource = Requests;
        }
	}
}