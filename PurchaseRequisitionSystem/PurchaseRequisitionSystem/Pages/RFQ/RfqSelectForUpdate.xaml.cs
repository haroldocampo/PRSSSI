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
using System.Collections;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for RfqSelectForUpdate.xaml
	/// </summary>
	public partial class RfqSelectForUpdate : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;
        private IQueryable<RfqModel> RfqList;
        private AsyncWork async;
        private Guid requestId;

		public RfqSelectForUpdate()
		{
			this.InitializeComponent();
            db = new PrsEntities();
        }

        public RfqSelectForUpdate(MainWindow mainWindow, Guid requestId) :this() {
            this.mainWindow = mainWindow;
            this.requestId = requestId;
            async = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            async.Do(LoadRfqs, LoadRfqs_Complete);
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select an item first.");
                return;
            }

            var rfqModel = (RfqModel)dgRequests.SelectedItem;
            RfqProcessUpdate rfqItemsDialog = new RfqProcessUpdate(mainWindow, rfqModel.Id, requestId);
            mainWindow.TFrame.ShowPage(rfqItemsDialog);
        }

        private void LoadRfqs() {
            var SentRFQs = (from r in db.RFQs
                            join pr in db.PurchaseRequests on r.RequestId equals pr.Id
                            where pr.IsQuoted == false && pr.Id == requestId
                            select new { pr, r });
            RfqList = SentRFQs.Select(x =>
            new RfqModel
            {
                Id = x.r.Id,
                PRNumber = x.pr.PRNo,
                VendorName = x.r.Vendor.Name,
                PRType = x.pr.PRType_IsStock == true ? "Stock" : "Non-Stock",
                RequestedBy = x.pr.User.LastName + ", " + x.pr.User.FirstName,
                DateRequired = x.pr.DateRequired,
                Status = x.pr.Status,
                IsQuoted = x.r.IsQuoted
            });
        }

        private void LoadRfqs_Complete() {
            dgRequests.ItemsSource = RfqList;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new RfqSelectRequest(mainWindow));
        }

        private void listItem_Loaded(object sender, RoutedEventArgs e) {
            var Item = sender as StackPanel;
            var model = Item.DataContext as RfqModel;

            if (model.IsQuoted) {
                Item.Background = new SolidColorBrush(Color.FromArgb(200,0,200,0));
            }
        }
	}
}