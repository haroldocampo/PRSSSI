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
using System.Windows.Shapes;
using PurchaseRequisitionSystem.Models;
using System.Linq;
using PurchaseRequisitionSystem.Background;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for CsSelectVendor.xaml
	/// </summary>
    public partial class CsSelectVendor : Window {
        private MainWindow mainWindow;
        private List<Models.CanvasItemModel> items;
        private Guid RequestId;
        private PrsEntities db;
        private IQueryable<RfqModel> RfqList { get; set; }
        private RfqModel model { get; set; }
        private int vendorNumber;
        private CsProcessRequest csProcessRequestDialog;
        private AsyncWork async;

        public CsSelectVendor() {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(CsSelectVendor_Loaded);
            db = new PrsEntities();
        }

        public CsSelectVendor(MainWindow mainWindow, List<CanvasItemModel> items, Guid RequestId, int vendorNumber, CsProcessRequest csProcessRequest) :this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.items = items;
            this.RequestId = RequestId;
            this.vendorNumber = vendorNumber;
            this.csProcessRequestDialog = csProcessRequest;
            async = new AsyncWork(mainWindow);
        }

        void CsSelectVendor_Loaded(object sender, RoutedEventArgs e) {
            var SentRFQs = (from r in db.RFQs
                            join pr in db.PurchaseRequests on r.RequestId equals pr.Id
                            where pr.Id == RequestId
                            select new { pr, r });
            RfqList = SentRFQs.Select(x =>
            new RfqModel
            {
                Id = x.r.Id,
                VendorName = x.r.Vendor.Name
            });
            lstVendors.ItemsSource = RfqList;
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            if(lstVendors.SelectedIndex == -1){
                MessageBox.Show("Please select a vendor first.");
                return;
            }

            model = (RfqModel)lstVendors.SelectedItem;

            async.Do(UpdateCanvasSheet, UpdateCanvasSheet_Complete);
        }

        private void UpdateCanvasSheet_Complete() {
            csProcessRequestDialog.UpdateCanvasList(items);

            switch (vendorNumber) {
                case 1:
                    csProcessRequestDialog.btnVendor1.Content = new TextBlock() { Text = model.VendorName, TextWrapping = TextWrapping.Wrap, Height = 75, Width = 95 };
                    break;
                case 2:
                    csProcessRequestDialog.btnVendor2.Content = new TextBlock() { Text = model.VendorName, TextWrapping = TextWrapping.Wrap, Height = 75, Width = 95 };
                    break;
                case 3:
                    csProcessRequestDialog.btnVendor3.Content = new TextBlock() { Text = model.VendorName, TextWrapping = TextWrapping.Wrap, Height = 75, Width = 95 };
                    break;
            }

            this.Close();
        }

        private void UpdateCanvasSheet() {
            
            var rfqItems = db.mRfqs_mItemsRequests.Where(x => x.RfqId == model.Id);

            // Iterate mItemsRequest
            foreach (var item in items) {
                // Iterate rfqItems
                foreach (var rfqItem in rfqItems) {
                    //If rfqItem equals mItemsRequestId insert price
                    if (item.mItemRequestId == rfqItem.mItemsRequestId) {
                        //Put value in the selected vendor
                        switch (vendorNumber) {
                            case 1:
                                item.PriceVendor1 = rfqItem.TotalPrice.HasValue ? rfqItem.TotalPrice.Value : 0;
                                item.mRfqId1 = rfqItem.Id;
                                break;
                            case 2:
                                item.PriceVendor2 = rfqItem.TotalPrice.HasValue ? rfqItem.TotalPrice.Value : 0;
                                item.mRfqId2 = rfqItem.Id;
                                break;
                            case 3:
                                item.PriceVendor3 = rfqItem.TotalPrice.HasValue ? rfqItem.TotalPrice.Value : 0;
                                item.mRfqId3 = rfqItem.Id;
                                break;
                        }
                    }
                }

            }
        }
    }
}