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
using System.Linq;
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.PDF;
using System.IO;
using Microsoft.Win32;
using PurchaseRequisitionSystem.Background;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for PoGenerateWindow.xaml
	/// </summary>
	public partial class PoGenerateWindow : Window
	{
        private MainWindow mainWindow;
        private PrsEntities db;
        private Guid RequestId;
        private IEnumerable<PoItemModel> PoItems;
        private IEnumerable<PoListModel> PoList;
        private PdfManager pdf;
        private UserModel CurrentUser;
        private UserModel RequestUser;
        private AsyncWork asyncLoad, asyncLoadItems;

        private int RequestNumber; // This variable for printing

		public PoGenerateWindow()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            pdf = new PdfManager();
            CurrentUser = (UserModel)App.Current.Properties["User"];
		}

        public PoGenerateWindow(MainWindow mainWindow, Guid requestId) : this(){
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.RequestId = requestId;
            asyncLoad = new AsyncWork(mainWindow); asyncLoadItems = new AsyncWork(mainWindow);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        //    if(User.UserTypeId == (int)UserTypes.VicePresident)
        //        btnApprove.IsEnabled = true;

            asyncLoad.Do(PopulatePoList, PopulatePoList_Complete);
        }

        private void PopulatePoList() {
            PoList = db.PurchaseOrders.Where(x => x.RequestId == RequestId).Select(x => new PoListModel()
            {
                Id = x.Id,
                PONumber = x.PoNumber,
                VendorId = x.VendorId,
                Vendor = db.Vendors.Where(y => y.Id == x.VendorId).FirstOrDefault().Name,
                VendorAddress = db.Vendors.Where(y => y.Id == x.VendorId).FirstOrDefault().Address,
                RequestedBy = db.PurchaseRequests.Where(y => y.Id == x.RequestId).FirstOrDefault().User.LastName + ", " + db.PurchaseRequests.Where(y => y.Id == x.RequestId).FirstOrDefault().User.FirstName,
                GrandTotal = db.mPo_mRfqItems.Where(y => y.PurchaseOrderId == x.Id).Sum(y => y.mRfqs_mItemsRequests.TotalPrice.HasValue ? y.mRfqs_mItemsRequests.TotalPrice.Value : 0),
                DeliveryDate = db.mPo_mRfqItems.Where(y => y.PurchaseOrderId == x.Id).FirstOrDefault().mRfqs_mItemsRequests.RFQ.DateRequired,
                DateRequired = db.PurchaseRequests.Where(y => y.Id == x.RequestId).FirstOrDefault().DateRequired
            });
        }

        private void PopulatePoList_Complete() {
            dgOrders.ItemsSource = PoList;
        }

        private PoListModel SelectedPoModel;
        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            if (dgOrders.SelectedIndex == -1) {
                MessageBox.Show("Please Select a Purchase Order.");
                return;
            }

            SelectedPoModel = (PoListModel)dgOrders.SelectedItem;

            asyncLoadItems.Do(PopulateDataForPrinting, PopulateDataForPrinting_Complete);
        }

        private void PopulateDataForPrinting() {
            var model = SelectedPoModel;
            PoItems = db.mPo_mRfqItems.Where(x => x.PurchaseOrderId == model.Id).Select(x => new PoItemModel()
            {
                Id = x.Id,
                ItemNumber = x.mRfqs_mItemsRequests.mItemsRequest.Item.ItemNumber,
                Description = x.mRfqs_mItemsRequests.mItemsRequest.Item.Description + ", " + x.mRfqs_mItemsRequests.mItemsRequest.Item.ItemBrand.Name + ", " + x.mRfqs_mItemsRequests.mItemsRequest.Item.CodeSize,
                Quantity = x.mRfqs_mItemsRequests.mItemsRequest.Quantity,
                UOM = x.mRfqs_mItemsRequests.mItemsRequest.Item.UnitOfMeasurement.Name,
                Price = x.mRfqs_mItemsRequests.UnitPrice.HasValue ? x.mRfqs_mItemsRequests.UnitPrice.Value : 0,
                Discount = x.mRfqs_mItemsRequests.Discount.HasValue ? x.mRfqs_mItemsRequests.Discount.Value : 0,
                TotalPrice = x.mRfqs_mItemsRequests.TotalPrice.HasValue ? x.mRfqs_mItemsRequests.TotalPrice.Value : 0 
            });

            var request = db.PurchaseRequests.Where(x => x.Id == RequestId).SingleOrDefault();
            RequestUser = db.Users.Where(x => x.Id == request.User.Id).Select(x => new UserModel()
            {
                CompanyId = x.CompanyId,
                CompanyName = x.Company.Name,
                Id = x.Id,
                DepartmentName = x.Department.Name,
                FirstName = x.FirstName,
                LastName = x.LastName,
                UserTypeId = x.UserTypeId,
            }).SingleOrDefault();
            RequestNumber = request.PRNo;
        }

        private void PopulateDataForPrinting_Complete() {
            PrintPurchaseOrder(SelectedPoModel);
        }

        private void PrintPurchaseOrder(PoListModel model) {
            int PoNumber = model.PONumber;
            string VendorName = model.Vendor;
            string VendorCode = "V-{0}".WithTokens(model.VendorId);
            DateTime DateCreated = DateTime.Now.Date;
            DateTime DeliveryDate = model.DeliveryDate.Date;

            MemoryStream purchaseOrderStream = new MemoryStream();
            pdf.RenderPurchaseOrder(purchaseOrderStream, PoNumber, PoItems, RequestNumber, VendorName, VendorCode, model.VendorAddress, DateCreated, DeliveryDate, RequestUser, CurrentUser);

            SaveFileDialog fd = new SaveFileDialog();
            fd.DefaultExt = ".pdf";
            fd.FileName = "PO" + SelectedPoModel.PONumber;
            fd.Title = "Where do you want to save the document?";
            fd.ShowDialog();

            if (fd.FileName.IsNullOrEmpty()) {
                return;
            }

            File.WriteAllBytes(fd.FileName, purchaseOrderStream.GetBuffer());
            this.Close();
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e) {
            if (dgOrders.SelectedIndex == -1) {
                MessageBox.Show("Please Select a Purchase Order.");
                return;
            }

            var model = (PoListModel)dgOrders.SelectedItem;

            var PurchaseOrder = db.PurchaseOrders.Where(x => x.Id == model.Id).SingleOrDefault();
            var CurrentRequest = db.PurchaseRequests.Where(x => x.Id == RequestId).SingleOrDefault();

            CurrentRequest.Status = "Purchase Order Approved";
            CurrentRequest.IsCanvased = true;
            
            PurchaseOrder.is_approved = true;
            PurchaseOrder.ApprovedBy = CurrentUser.Id;

            db.SaveChanges();

            mainWindow.TFrame.ShowPage(new PoSelectRequest(mainWindow));

            this.Close();
        }
	}
}