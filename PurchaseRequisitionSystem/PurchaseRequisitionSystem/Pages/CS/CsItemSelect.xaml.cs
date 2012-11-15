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

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for CsItemSelect.xaml
	/// </summary>
	public partial class CsItemSelect : UserControl
	{
        PrsEntities db;
        private CsProcessRequest processDialog;
        private MainWindow mainWindow;
        private AsyncWork asyncLoad;
        public Guid RequestId { get; set; }
        private List<ItemModel> CsItems;

		public CsItemSelect()
		{
			this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(CsItemSelect_Loaded);
            db = new PrsEntities();
		}

        public CsItemSelect(MainWindow mainWindow) : this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            asyncLoad = new AsyncWork(mainWindow);
            processDialog = new CsProcessRequest(mainWindow);
            processDialog.items = new List<CanvasItemModel>();
        }

        public CsItemSelect(MainWindow mainWindow, Guid requestId) : this(mainWindow) {
            this.RequestId = requestId;
        }

        void CsItemSelect_Loaded(object sender, RoutedEventArgs e) {
            asyncLoad.Do(LoadData, LoadData_Conplete);
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            PopulateDataForProcess();
        }

        private void LoadData(){
            CsItems = new List<ItemModel>();
            IQueryable<mItemsRequest> AllItems = db.mItemsRequests;
            var itemRequests = AllItems.Where(x => x.RequestId == RequestId);
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
                    Status = itemRequest.IsQuoteSent == true ? "Quoted" : "Not-Quoted",
                    UOM = item.UnitOfMeasurement.Name
                };
                CsItems.Add(requestItem);
            }
        }

        private void LoadData_Conplete() {
            foreach (var item in CsItems) {
                dgItemsFrom.Items.Add(item);
            }
        }

        private void PopulateDataForProcess() { // Create a control that selects the items for Cs Processing
            var itemRequests = dgItemsTo.Items.Cast<ItemModel>();

            if (!itemRequests.Any()) {
                MessageBox.Show("There is no data to process");
                return;
            }

            foreach (var itemRequest in itemRequests) {
                var requestItem = new CanvasItemModel()
                {
                    mItemRequestId = itemRequest.ext_id,
                    ItemNumber = itemRequest.ItemNumber,
                    ItemDescription = itemRequest.Description,
                    Quantity = itemRequest.Quantity,
                    UOM = itemRequest.UOM,
                    PriceVendor1 = 0,
                    PriceVendor2 = 0,
                    PriceVendor3 = 0
                };
                processDialog.RequestId = this.RequestId;
                processDialog.items.Add(requestItem);
            }
            mainWindow.TFrame.ShowPage(processDialog);
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

        private void dgItemsFrom_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            btnInsertItem_Click(this, e);
        }

        private void dgItemsTo_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            btnRemoveItem_Click(this, e);
        }
    }
}