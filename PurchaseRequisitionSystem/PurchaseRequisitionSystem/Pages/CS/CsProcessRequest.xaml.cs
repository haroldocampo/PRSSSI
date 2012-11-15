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
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Background;
using System.Linq;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for CsProcessRequest.xaml
	/// </summary>
	public partial class CsProcessRequest : UserControl
	{
        private MainWindow mainWindow;
        public List<CanvasItemModel> items { get; set; }
        public Guid RequestId { get; set; }
        private PrsEntities db;
        private AsyncWork async;
        private UserModel User;

        private double GrandTotal1;
        private double GrandTotal2;
        private double GrandTotal3;

		public CsProcessRequest()
		{
			this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(CsProcessRequest_Loaded);
            db = new PrsEntities();
            User = (UserModel)App.Current.Properties["User"];
		}

        public CsProcessRequest(MainWindow mainWindow) : this(){
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow);
        }

        void CsProcessRequest_Loaded(object sender, RoutedEventArgs e) {
            dgRequests.ItemsSource = items;
        }

        private void ShowVendorDialog(int vendorNumber){
            //Clear Values
            ClearList(vendorNumber);

            CsSelectVendor vendor = new CsSelectVendor(mainWindow,items,RequestId,vendorNumber, this);
            vendor.ShowDialog();
        }

        private void ClearList(int vendorNumber) {
            foreach (var item in items) {
                switch (vendorNumber) {
                    case 1:
                        item.IsChecked1 = false;
                        item.mCsId1 = 0;
                        item.mRfqId1 = 0;
                        item.PriceVendor1 = 0;
                        break;
                    case 2:
                        item.IsChecked2 = false;
                        item.mCsId2 = 0;
                        item.mRfqId2 = 0;
                        item.PriceVendor2 = 0;
                        break;
                    case 3:
                        item.IsChecked3 = false;
                        item.mCsId3 = 0;
                        item.mRfqId3 = 0;
                        item.PriceVendor3 = 0;
                        break;
                }
            }

            dgRequests.ItemsSource = items;
        }

        private void btnVendor1_Click_1(object sender, RoutedEventArgs e) {
            ShowVendorDialog(1);
        }

        private void btnVendor2_Click(object sender, RoutedEventArgs e) {
            ShowVendorDialog(2);
        }

        private void btnVendor3_Click(object sender, RoutedEventArgs e) {
            ShowVendorDialog(3);
        }

        public void UpdateCanvasList(List<Models.CanvasItemModel> newItems) {
            //Reset Values
            dgRequests.ItemsSource = null;
            GrandTotal1 = 0;
            GrandTotal2 = 0;
            GrandTotal3 = 0;

            // Update Items
            dgRequests.ItemsSource = newItems;

            foreach (var item in newItems) {
                GrandTotal1 += item.PriceVendor1;
                GrandTotal2 += item.PriceVendor2;
                GrandTotal3 += item.PriceVendor3;
            }

            txtGrandPriceV1.Text = GrandTotal1.ToString();
            txtGrandPriceV2.Text = GrandTotal2.ToString();
            txtGrandPriceV3.Text = GrandTotal3.ToString();

            items = newItems;
        }

        private void SaveToDatabase(List<Models.CanvasItemModel> newItems) {
            // Get RequestId
            Guid RequestId = Guid.Empty; bool RequestDataDone = false;
            Guid CanvasId = Guid.NewGuid();
            // Create Items for CS items and GET RequestId
            GenerateManyCSItems(newItems, ref RequestId, ref RequestDataDone, CanvasId);
            // Create Canvas Sheet
            GenerateCanvasSheet(RequestId, CanvasId);
            // Mark Purchase Request Status as Pending CS Approval
            var Request = db.PurchaseRequests.Where(x => x.Id == RequestId).SingleOrDefault();
            Request.Status = "Pending CS Approval";
            //Generate PDF for Canvas Sheet

            //Save to the Database
            db.SaveChanges();
        }

        private void GenerateManyCSItems(List<Models.CanvasItemModel> newItems, ref Guid RequestId, ref bool RequestDataDone, Guid CanvasId) {
            foreach (var item in newItems) {
                // Popultae many CS items
                if (item.mRfqId1 != 0) {
                    var csItem1 = db.mCS_mRfq_mItemsRequest.CreateObject();
                    csItem1.CanvasSheetId = CanvasId;
                    csItem1.mRfqs_mItemsRequestsId = item.mRfqId1;
                    csItem1.VendorNumber = 1;
                    db.mCS_mRfq_mItemsRequest.AddObject(csItem1);
                }

                if (item.mRfqId2 != 0) {
                    var csItem2 = db.mCS_mRfq_mItemsRequest.CreateObject();
                    csItem2.CanvasSheetId = CanvasId;
                    csItem2.mRfqs_mItemsRequestsId = item.mRfqId2;
                    csItem2.VendorNumber = 2;
                    db.mCS_mRfq_mItemsRequest.AddObject(csItem2);
                }

                if (item.mRfqId3 != 0) {
                    var csItem3 = db.mCS_mRfq_mItemsRequest.CreateObject();
                    csItem3.CanvasSheetId = CanvasId;
                    csItem3.mRfqs_mItemsRequestsId = item.mRfqId3;
                    csItem3.VendorNumber = 3;
                    db.mCS_mRfq_mItemsRequest.AddObject(csItem3);
                }
                // ----------------------------------------------------------
                if (!RequestDataDone) {
                    // Get RequestId
                    RequestId = db.mItemsRequests.Where(x => x.Id == item.mItemRequestId).SingleOrDefault().RequestId;
                    RequestDataDone = true;
                }
            }
        }

        private void GenerateCanvasSheet(Guid RequestId, Guid CanvasId) {
            var newCanvasSheet = db.CanvasSheets.CreateObject();
            newCanvasSheet.Id = CanvasId;
            newCanvasSheet.RequestId = RequestId;
            newCanvasSheet.PreparedBy = User.Id;
            newCanvasSheet.DateCreated = DateTime.Now.Date;
            db.CanvasSheets.AddObject(newCanvasSheet);
        }

        private void SaveToDatabase_Complete() {
            MessageBox.Show("Canvas Sheet has been saved","Complete", MessageBoxButton.OK,MessageBoxImage.Information);
            mainWindow.TFrame.ShowPage(new CsSelectRequest(mainWindow));
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new CsItemSelect(mainWindow,RequestId));
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            async.Do(SaveToDatabase, items, SaveToDatabase_Complete);
        }
	}
}