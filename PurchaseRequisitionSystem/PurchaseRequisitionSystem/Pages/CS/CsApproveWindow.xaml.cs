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
using PurchaseRequisitionSystem.Background;
using PurchaseRequisitionSystem.Models;
using System.Linq;
using PurchaseRequisitionSystem.PDF;
using System.IO;
using Microsoft.Win32;
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for CsApproveWindow.xaml
	/// </summary>
	public partial class CsApproveWindow : Window
	{
        private Guid CanvasId;
        private Guid RequestId;
        private int CanvasNumber;
        private MainWindow mainWindow;
        private AsyncWork asyncLoad, asyncApprove;
        private List<CanvasItemModel> items;
        private PrsEntities db;
        private UserModel User;
        private PurchaseRequest CurrentRequest;

        // Purchase Order Objects
        private PurchaseOrder Po1;
        private PurchaseOrder Po2;
        private PurchaseOrder Po3;
        private bool IsPo1Empty;
        private bool IsPo2Empty;
        private bool IsPo3Empty;

        // Grand total values
        private double GrandTotal1;
        private double GrandTotal2;
        private double GrandTotal3;

		public CsApproveWindow()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            User = (UserModel)App.Current.Properties["User"];
            // Initialize Purchase Order Objects
            Po1 = db.PurchaseOrders.CreateObject(); Po2 = db.PurchaseOrders.CreateObject(); Po3 = db.PurchaseOrders.CreateObject();
		}

        public CsApproveWindow(MainWindow mainWindow, Guid canvasId, Guid RequestId) :this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.CanvasId = canvasId;
            this.RequestId = RequestId;
            asyncLoad = new AsyncWork(mainWindow); asyncApprove = new AsyncWork(mainWindow);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Let the PR Manager do this too
            //if (User.UserTypeId != (int)UserTypes.VicePresident) {
            //    btnFinish.IsEnabled = false;
            //}

            asyncLoad.Do(PopulateCanvasSheet, PopulateCanvasSheet_Complete);
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e) {
            asyncApprove.Do(DeployPurchaseOrderAndCanvasSheet, DeployPurchaseOrderAndCanvasSheet_Complete);
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e) {
            PrintCanvasSheet();
        }

        private void PopulateCanvasSheet(){
            // Instantiation
            items = new List<CanvasItemModel>();
            //Get current canvas sheet
            var currentCanvasSheet = db.mCS_mRfq_mItemsRequest.Where(x=>x.CanvasSheetId == CanvasId);
            CanvasNumber = currentCanvasSheet.FirstOrDefault().CanvasSheet.CsNumber;
            //Get all items
            IQueryable<mItemsRequest> AllItemsRequests = db.mItemsRequests;
            //Get items based on RequestId
            var itemRequests = AllItemsRequests.Where(x => x.RequestId == RequestId);
            //var itemRequests = currentCanvasSheet.
            //Iterate through all Items
            foreach (var itemRequest in itemRequests) {

                //Declare initial values for Id
                int _mRfq1 = 0,_mRfq2 = 0,_mRfq3 = 0;

                //Declare initial values for Id
                int _mCsId1 = 0, _mCsId2 = 0, _mCsId3 = 0;

                //Declare initial values for total price
                double _PriceVendor1 = 0,_PriceVendor2 = 0,_PriceVendor3 = 0;

                //Iterate CS Items
                foreach(var tmpCsItem in currentCanvasSheet){
                    //Check if item matches CS item
                    if (itemRequest.Id == tmpCsItem.mRfqs_mItemsRequests.mItemsRequest.Id) {
                        // Populate items based ib column number
                        switch (tmpCsItem.VendorNumber) {
                            case 1: _mRfq1 = tmpCsItem.mRfqs_mItemsRequestsId; _PriceVendor1 = tmpCsItem.mRfqs_mItemsRequests.TotalPrice.HasValue ? tmpCsItem.mRfqs_mItemsRequests.TotalPrice.Value : 0; _mCsId1 = tmpCsItem.Id;
                                break;
                            case 2: _mRfq2 = tmpCsItem.mRfqs_mItemsRequestsId; _PriceVendor2 = tmpCsItem.mRfqs_mItemsRequests.TotalPrice.HasValue ? tmpCsItem.mRfqs_mItemsRequests.TotalPrice.Value : 0; _mCsId2 = tmpCsItem.Id;
                                break;
                            case 3: _mRfq3 = tmpCsItem.mRfqs_mItemsRequestsId; _PriceVendor3 = tmpCsItem.mRfqs_mItemsRequests.TotalPrice.HasValue ? tmpCsItem.mRfqs_mItemsRequests.TotalPrice.Value : 0; _mCsId3 = tmpCsItem.Id;
                                break;
                        }

                    }
                }

                IQueryable<Item> tmpItem = db.Items;
                var item = tmpItem.Where(x => x.Id == itemRequest.ItemId).SingleOrDefault();
                string LastPurchaseDate = item.LastPurchasedDate.HasValue ? item.LastPurchasedDate.Value.ToShortDateString() : "No Record";
                double LastPurchasePrice = item.LastPurchasePrice.HasValue ? item.LastPurchasePrice.Value : 0;
                var requestItem = new CanvasItemModel()
                {
                    mItemRequestId = itemRequest.Id,
                    ItemNumber = item.ItemNumber,
                    ItemDescription = item.Description + ", " + item.ItemBrand.Name + ", " + item.CodeSize + "\n\n LPD: " + LastPurchaseDate + "; LPP: " + LastPurchasePrice,
                    Quantity = itemRequest.Quantity,
                    UOM = item.UnitOfMeasurement.Name,
                    PriceVendor1 = _PriceVendor1,
                    PriceVendor2 = _PriceVendor2,
                    PriceVendor3 = _PriceVendor3,
                    mRfqId1 = _mRfq1,
                    mRfqId2 = _mRfq2,
                    mRfqId3 = _mRfq3,
                    mCsId1 = _mCsId1,
                    mCsId2 = _mCsId2,
                    mCsId3 = _mCsId3
                };

                // If the item exists in the context
                if (requestItem.mCsId1 != 0 || requestItem.mCsId2 != 0 || requestItem.mCsId3 != 0) {
                    items.Add(requestItem);

                    // Sum up grand total
                    GrandTotal1 += requestItem.PriceVendor1;
                    GrandTotal2 += requestItem.PriceVendor2;
                    GrandTotal3 += requestItem.PriceVendor3;
                }

                //Populate Vendor Names
                PopulateVendorNames();
            }
        }

        private void PopulateVendorNames() {
            foreach (var tmpitem in items) {
                if (tmpitem.mRfqId1 != 0 && VendorName1.IsNullOrEmpty()) {
                    VendorName1 = db.mRfqs_mItemsRequests.Where(x => x.Id == tmpitem.mRfqId1).SingleOrDefault().RFQ.Vendor.Name;
                }

                if (tmpitem.mRfqId2 != 0 && VendorName2.IsNullOrEmpty()) {
                    VendorName2 = db.mRfqs_mItemsRequests.Where(x => x.Id == tmpitem.mRfqId2).SingleOrDefault().RFQ.Vendor.Name;
                }

                if (tmpitem.mRfqId3 != 0 && VendorName3.IsNullOrEmpty()) {
                    VendorName3 = db.mRfqs_mItemsRequests.Where(x => x.Id == tmpitem.mRfqId3).SingleOrDefault().RFQ.Vendor.Name;
                }
            }
        }

        string VendorName1 = "", VendorName2 = "", VendorName3 = "";
        private void PrintCanvasSheet() {
            PdfManager pdf = new PdfManager();
            CurrentRequest = db.PurchaseRequests.Where(x=>x.Id == RequestId).SingleOrDefault();

            string[] VendorName = {VendorName1, VendorName2, VendorName3};

            UserModel RequestedBy = db.Users.Where(x => x.Id == CurrentRequest.RequestedBy).Select(x => new UserModel()
            {
                DepartmentName = x.Department.Name,
                CompanyName = x.Company.Name,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).SingleOrDefault();

            MemoryStream canvasStream = new MemoryStream();
            pdf.RenderCanvasSheet(canvasStream, items, User,RequestedBy, CurrentRequest, VendorName);

            SaveFileDialog fd = new SaveFileDialog();
            fd.DefaultExt = ".pdf";
            fd.FileName = "CS" + CanvasNumber;
            fd.Title = "Where do you want to save the document?";
            fd.ShowDialog();

            if (fd.FileName.IsNullOrEmpty()) {
                return;
            }

            File.WriteAllBytes(fd.FileName, canvasStream.GetBuffer());
        }

        private void PopulateCanvasSheet_Complete() {
            dgCanvasItems.ItemsSource = items;

            txtGrandPriceV1.Text = GrandTotal1.ToString();
            txtGrandPriceV2.Text = GrandTotal2.ToString();
            txtGrandPriceV3.Text = GrandTotal3.ToString();

            tbVendor1.Text = VendorName1.IsNullOrEmpty() ? "-----" : VendorName1; tbVendor2.Text = VendorName2.IsNullOrEmpty() ? "-----" : VendorName2; tbVendor3.Text = VendorName3.IsNullOrEmpty() ? "-----" : VendorName3;
        }

        private void DeployPurchaseOrderAndCanvasSheet() {
            InitializePurchaseOrders();

            Guid CsId = Guid.Empty;
            foreach (var item in items) {
                //Get CSId
                if (CsId == Guid.Empty) {
                    if(item.mCsId1 != 0)
                        CsId = db.mCS_mRfq_mItemsRequest.Where(x => x.Id == item.mCsId1).FirstOrDefault().CanvasSheetId;
                    if (item.mCsId2 != 0)
                        CsId = db.mCS_mRfq_mItemsRequest.Where(x => x.Id == item.mCsId2).FirstOrDefault().CanvasSheetId;
                    if (item.mCsId3 != 0)
                        CsId = db.mCS_mRfq_mItemsRequest.Where(x => x.Id == item.mCsId3).FirstOrDefault().CanvasSheetId;
                }
                // Update many CS items and Generate PO Order
                RenderCsUpdateAndPoOrder(item);
            }

            // Set Canvas to Approved
            var CanvasSheet = db.CanvasSheets.Where(x => x.Id == CsId).SingleOrDefault();
            CanvasSheet.ApprovedBy = User.Id;
            CanvasSheet.is_approved = true;

            //Set Request Status
            CurrentRequest = db.PurchaseRequests.Where(x=>x.Id == RequestId).SingleOrDefault();
            CurrentRequest.Status = "CS Approved";
            CurrentRequest.IsPoReady = true;

            db.SaveChanges();
        }

        private void InitializePurchaseOrders() {
            Po1.Id = Guid.NewGuid();
            Po2.Id = Guid.NewGuid();
            Po3.Id = Guid.NewGuid();
            Po1.RequestId = RequestId;
            Po2.RequestId = RequestId;
            Po3.RequestId = RequestId;
            Po1.DateCreated = DateTime.Now;
            Po2.DateCreated = DateTime.Now;
            Po3.DateCreated = DateTime.Now;
        }

        private void RenderCsUpdateAndPoOrder(CanvasItemModel item) {
            if (item.mCsId1 != 0 && item.IsChecked1) {
                var csItem1 = db.mCS_mRfq_mItemsRequest.Where(x => x.Id == item.mCsId1).SingleOrDefault();
                csItem1.IsSelected = true;

                var model1 = db.mRfqs_mItemsRequests.Where(x => x.Id == item.mRfqId1).SingleOrDefault();
                
                // Mark item last purchase price
                MarkItemLastPurchasePrice(item, Scenario.One);

                if (!IsPo1Empty) { // Select a vendor and Mark PO as existing
                    Po1.VendorId = model1.RFQ.VendorId;
                    db.PurchaseOrders.AddObject(Po1);
                    IsPo1Empty = true;
                }
                
                // Add many to many RFQ Item
                db.mPo_mRfqItems.AddObject(new mPo_mRfqItems() { Id = Guid.NewGuid(), mRfqs_mItemsRequestsId = item.mRfqId1, PurchaseOrderId = Po1.Id, });
            }

            if (item.mCsId2 != 0 && item.IsChecked2) {
                var csItem2 = db.mCS_mRfq_mItemsRequest.Where(x => x.Id == item.mCsId2).SingleOrDefault();
                csItem2.IsSelected = true;

                var model2 = db.mRfqs_mItemsRequests.Where(x => x.Id == item.mRfqId2).SingleOrDefault();

                // Mark item last purchase price
                MarkItemLastPurchasePrice(item, Scenario.Two);

                if (!IsPo2Empty) { // Select a vendor and Mark PO as existing
                    Po2.VendorId = model2.RFQ.VendorId;
                    db.PurchaseOrders.AddObject(Po2);
                    IsPo2Empty = true;
                }
                // Add many to many RFQ Item
                db.mPo_mRfqItems.AddObject(new mPo_mRfqItems() { Id= Guid.NewGuid(), mRfqs_mItemsRequestsId = item.mRfqId2, PurchaseOrderId = Po2.Id, });
            }

            if (item.mCsId3 != 0 && item.IsChecked3) {
                var csItem3 = db.mCS_mRfq_mItemsRequest.Where(x => x.Id == item.mCsId3).SingleOrDefault();
                csItem3.IsSelected = true;

                var model3 = db.mRfqs_mItemsRequests.Where(x => x.Id == item.mRfqId3).SingleOrDefault();

                // Mark item last purchase price
                MarkItemLastPurchasePrice(item, Scenario.Three);

                if (!IsPo3Empty) { // Select a vendor and Mark PO as existing
                    Po3.VendorId = model3.RFQ.VendorId;
                    db.PurchaseOrders.AddObject(Po3);
                    IsPo3Empty = true;
                }
                // Add many to many RFQ Item
                db.mPo_mRfqItems.AddObject(new mPo_mRfqItems() { Id = Guid.NewGuid(), mRfqs_mItemsRequestsId = item.mRfqId3, PurchaseOrderId = Po3.Id, });
            }
        }

        private void MarkItemLastPurchasePrice(CanvasItemModel item, Scenario scene) {
            //Select item and mark last purchase price
            var itemRequest = db.mItemsRequests.Where(x => x.Id == item.mItemRequestId).SingleOrDefault();
            var selectedItem = db.Items.Where(x => x.Id == itemRequest.ItemId).SingleOrDefault();
            //Mark Purchase Price
            if (scene == Scenario.One) {
                selectedItem.LastPurchasePrice = item.PriceVendor1 / itemRequest.Quantity;
                itemRequest.Price = item.PriceVendor1;
            }
            else if (scene == Scenario.Two) {
                selectedItem.LastPurchasePrice = item.PriceVendor2 / itemRequest.Quantity;
                itemRequest.Price = item.PriceVendor2;
            }
            else if (scene == Scenario.Three) {
                selectedItem.LastPurchasePrice = item.PriceVendor3 / itemRequest.Quantity;
                itemRequest.Price = item.PriceVendor3;
            }
        }

        private void DeployPurchaseOrderAndCanvasSheet_Complete() {
            mainWindow.TFrame.ShowPage(new CsApprove(mainWindow));
            this.Close();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private enum Scenario {
            One = 1,
            Two = 2,
            Three = 3
        }
	}
}