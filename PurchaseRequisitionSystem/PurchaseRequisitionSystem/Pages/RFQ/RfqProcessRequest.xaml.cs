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
using System.Linq;
using PurchaseRequisitionSystem.Background;
using PurchaseRequisitionSystem.PDF;
using System.IO;
using Microsoft.Win32;
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for RfqProcessRequest.xaml
	/// </summary>
	public partial class RfqProcessRequest : UserControl
	{
        public IEnumerable<VendorModel> Vendors { get; set; }
        public List<ItemModel> Items { get; set; }

        private AsyncWork asyncOnSave, asyncOnLoad, asyncOnSearch;
        private MainWindow mainWindow;
        private PrsEntities db;
        private UserModel CurrentUser;
        private VendorModel selectedVendor;
        private int RfqNo;
        private string FolderPath;
        private PurchaseRequest CurrentRequest;
        IEnumerable<ItemModel> items;
        private IEnumerable<VendorModel> selectedVendors;

        DateTime DateRequired;
        string searchText;
        IEnumerable<VendorModel> searchModel;
        private DateTime DateRequested;

		public RfqProcessRequest()
		{
			this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(RfqProcessRequest_Loaded);
            db = new PrsEntities();
            CurrentUser = (UserModel)App.Current.Properties["User"];
		}

        public RfqProcessRequest(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
            asyncOnSave = new AsyncWork(mainWindow); asyncOnLoad = new AsyncWork(mainWindow); asyncOnSearch = new AsyncWork(mainWindow);
        }

        void RfqProcessRequest_Loaded(object sender, RoutedEventArgs e) {
            asyncOnLoad.Do(LoadData, LoadData_Complete);
        }

        private void btnInsertItem_Click(object sender, RoutedEventArgs e) {
            if (dgItemsFrom.SelectedIndex == -1) {
                MessageBox.Show("Please select an item too insert");
                return;
            }

            var selectedItem = dgItemsFrom.SelectedItem;
            var selectedModel = (ItemModel)selectedItem;
            dgItemsTo.Items.Add(selectedModel);
            dgItemsFrom.Items.Remove(selectedItem);
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e) {
            if (dgItemsTo.SelectedIndex == -1) {
                MessageBox.Show("Please select an item too remove");
                return;
            }

            var selectedItem = dgItemsTo.SelectedItem;
            var selectedModel = (ItemModel)selectedItem;
            dgItemsFrom.Items.Add(selectedModel);
            dgItemsTo.Items.Remove(selectedItem);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            searchText = txtSearch.Text;
            asyncOnSearch.Do(SearchVendor, SearchVendor_Complete);
        }
        
        private void btnSendAndSave_Click(object sender, RoutedEventArgs e) {
            if (dgItemsTo.Items.Count <= 0) {
                MessageBox.Show("Please insert some items for quotation.");
                return;
            }

            if (dgVendors.SelectedIndex == -1) {
                MessageBox.Show("Please select a vendor");
                return;
            }

            if (txtDateRequired.Text == "" || !DateTime.TryParse(txtDateRequired.Text, out DateRequired)) {
                MessageBox.Show("Please input a correct date.");
                return;
            }

            DateRequested = DateTime.Parse(txtDateRequired.Text);

            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            fbd.ShowNewFolderButton = true;
            fbd.ShowDialog();
            FolderPath = fbd.SelectedPath;

            if (FolderPath.IsNullOrEmpty()) {
                return;
            }

            selectedVendors = dgVendors.SelectedItems.OfType<VendorModel>();
            DateRequired = DateTime.Parse(txtDateRequired.Text);
            items = dgItemsTo.Items.Cast<ItemModel>();

            asyncOnSave.Do(SaveRfqToDb, SaveRfqToDb_Complete);
        }

        private void SaveRfqToDb() {
            //------------------Database Operation-------------------------
            foreach (var vendor in selectedVendors) {
                selectedVendor = vendor;
                // Create RFQ
                Guid rfqId = Guid.NewGuid();
                var rfq = db.RFQs.CreateObject();
                rfq.Id = rfqId;
                rfq.IssuedBy = CurrentUser.Id;
                rfq.DateRequired = DateRequired;
                rfq.date_created = DateTime.Now;
                rfq.VendorId = vendor.Id;

                bool IsPrDataTaken = false;
                Guid RequestId = Guid.Empty;
                //Add Items
                foreach (var item in items) {
                    //Add to quotation item list
                    var mRfqItems = db.mRfqs_mItemsRequests.CreateObject();
                    mRfqItems.RfqId = rfqId;
                    mRfqItems.mItemsRequestId = item.ext_id;

                    //Mark itemrequest as quoted
                    IQueryable<mItemsRequest> allItemRequest = db.mItemsRequests;
                    var itemRequest = allItemRequest.Where(x => x.Id == item.ext_id).SingleOrDefault();
                    itemRequest.IsQuoteSent = true;

                    //Get Purchase Request Data for printing
                    if (!IsPrDataTaken) {
                        CurrentRequest = itemRequest.PurchaseRequest;
                        CurrentRequest.Status = "Pending Quotation";
                        RequestId = CurrentRequest.Id;
                        IsPrDataTaken = true;
                    }
                    //Add object
                    mRfqItems.RequestId = RequestId;
                    db.mRfqs_mItemsRequests.AddObject(mRfqItems);
                }
                // Add object
                rfq.RequestId = RequestId;
                db.RFQs.AddObject(rfq);
                //Save To Database
                db.SaveChanges();

                RfqNo = db.RFQs.Where(x => x.Id == rfqId).SingleOrDefault().RfqNumber;
                DeployAndRenderRfq();
            }
        }

        private void SaveRfqToDb_Complete() {
            mainWindow.TFrame.ShowPage(new RfqSelectRequest(mainWindow));
        } 

        private void DeployAndRenderRfq() {
            PdfManager manager = new PdfManager();
            MemoryStream stream = new MemoryStream();
            manager.RenderRfq(stream, selectedVendor, items, CurrentUser, RfqNo, CurrentRequest, DateRequested);
            string rfqFileName = "RFQ" + RfqNo + " - " + selectedVendor.VendorName + ".pdf";
            string fileName = System.IO.Path.Combine(FolderPath,rfqFileName); // folderpath + rfqnumber + vendorname + fileextenstion
            File.WriteAllBytes(fileName, stream.GetBuffer());
        }

        private void SearchVendor() {
            IQueryable<VendorModel> tmpVendors = db.Vendors.Select(x => new VendorModel
            {
                Id = x.Id,
                VendorName = x.Name,
                Terms = x.Terms,
                ContactPerson = x.ContactPerson,
                Address = x.Address,
                Email = x.Email,
                Telephone = x.Telephone,
                FaxNumber = x.FaxNumber
            });

            searchModel = tmpVendors.Where(x=>x.VendorName.Contains(searchText));
        }

        private void SearchVendor_Complete() {
            dgVendors.ItemsSource = searchModel;
        }

        private void LoadData() {
            Vendors = db.Vendors.Select(x => new VendorModel
            {
                Id = x.Id,
                VendorName = x.Name,
                Terms = x.Terms,
                ContactPerson = x.ContactPerson,
                Address = x.Address,
                Email = x.Email,
                Telephone = x.Telephone,
                FaxNumber = x.FaxNumber
            });
        }

        private void LoadData_Complete() {
            dgVendors.ItemsSource = Vendors;

            foreach (var item in Items) {
                dgItemsFrom.Items.Add(item);
            }
        }

        private void dgItemsFrom_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            btnInsertItem_Click(this, e);
        }

        private void dgItemsTo_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            btnRemoveItem_Click(this, e);
        }
	}
}