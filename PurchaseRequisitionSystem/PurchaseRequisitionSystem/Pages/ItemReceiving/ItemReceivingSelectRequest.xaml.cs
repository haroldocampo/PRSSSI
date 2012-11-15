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
using PurchaseRequisitionSystem.Background;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for ItemReceivingSelectRequest.xaml
	/// </summary>
	public partial class ItemReceivingSelectRequest : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;
        private AsyncWork asyncMark, asyncLoad;
        private IQueryable<RequestModel> Requests;
        private RequestModel model;
        private ViewRequest viewDialog;

		public ItemReceivingSelectRequest()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public ItemReceivingSelectRequest(MainWindow mainWindow) : this(){
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            asyncMark = new AsyncWork(mainWindow); asyncLoad = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            asyncLoad.Do(PopulateRequests, PopulateRequests_Complete);
        }

        private void PopulateRequests() {
            var ApprovalRequestsP = (from r in db.PurchaseRequests
                                     where r.IsQuoted == true && r.IsCanvased == true && r.IsPoReady == true && r.IsFinished == false
                                     select r); // Select only Approved
            Requests = ApprovalRequestsP.Select(x =>
            new RequestModel
            {
                Id = x.Id,
                PRNumber = x.PRNo,
                Purpose = x.Purpose,
                PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                DateRequired = x.DateRequired,
                Status = x.Status
            });
        }

        private void PopulateRequests_Complete() {
            dgRequests.ItemsSource = Requests;
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            if(dgRequests.SelectedIndex == -1){
                MessageBox.Show("Please select a request.");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;
            mainWindow.TFrame.ShowPage(new ItemReceivingProcess(mainWindow, model.Id));
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request.");
                return;
            }

            model = (RequestModel)dgRequests.SelectedItem;

            asyncMark.Do(MarkRequestAsFinished, MarkRequestAsFinished_Complete);
        }

        private void MarkRequestAsFinished() {
            var request = db.PurchaseRequests.Where(x => x.Id == model.Id).SingleOrDefault();

            request.IsFinished = true;
            request.Status = "Complete";

            db.SaveChanges();
        }

        private void MarkRequestAsFinished_Complete() {
            mainWindow.TFrame.ShowPage(new ItemReceivingSelectRequest(mainWindow));
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
	}
}