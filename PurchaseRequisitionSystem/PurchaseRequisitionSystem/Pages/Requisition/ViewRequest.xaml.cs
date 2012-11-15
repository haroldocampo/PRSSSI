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
using PurchaseRequisitionSystem.Background;
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for ViewRequest.xaml
	/// </summary>
	public partial class ViewRequest : Window
	{
        private RequestModel model;
        private PrsEntities db;
        private MainWindow mainWindow;
        private AsyncWork async,async2;
        private List<ItemModel> items{get;set;}
        private UserModel User;
        private bool EnableEditing = false;

		public ViewRequest()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            this.Loaded += new RoutedEventHandler(ViewRequest_Loaded);
            User = App.Current.Properties["User"] as UserModel;
		}

        public ViewRequest(RequestModel model, MainWindow mainWindow) :this() {
            // TODO: Complete member initialization
            this.model = model;
            this.mainWindow = mainWindow;
            this.IsEnabled = false;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow);
        }

        void ViewRequest_Loaded(object sender, RoutedEventArgs e) {
            if (User.UserTypeId == (int)UserTypes.Purchasing) {
                EnableEditing = true;
            }
            tbPrNumber.Text = model.PRNumberModel;
            tbPrType.Text = model.PRType;
            tbDateRequired.Text = model.DateRequired.ToLongDateString();
            tbPurpose.Text = model.Purpose;
            tbRequestedBy.Text = model.RequestedBy;
            tbDateRequested.Text = model.DateRequested.ToShortDateString();
            tbStatus.Text = model.Status;
            async.Do(PopulateItems, PopulateItems_Complete);
        }

        public void RefreshData() {
            async2.Do(PopulateItems, PopulateItems_Complete);
        }

        private void PopulateItems() {
            items = new List<ItemModel>();
            IQueryable<mItemsRequest> AllItems = db.mItemsRequests;
            var itemRequests = AllItems.Where(x => x.RequestId == model.Id);
            foreach (var itemRequest in itemRequests) {
                IQueryable<Item> tmpItem = db.Items;
                var item = tmpItem.Where(x => x.Id == itemRequest.ItemId).SingleOrDefault();
                var requestItem = new ItemModel()
                {
                    Id = item.Id,
                    ext_id = itemRequest.Id,
                    ItemNumber = item.ItemNumber,
                    Description = item.Description,
                    Brand = item.ItemBrand.Name,
                    CodeSize = item.CodeSize,
                    CostObjective = itemRequest.CostObjective.Name,
                    CostObjectiveId = itemRequest.CostObjective.Id,
                    Quantity = itemRequest.Quantity,
                    StockOnHand = itemRequest.StockOnHand,
                    EnableEdit = EnableEditing && !item.is_active.Value
                };
                items.Add(requestItem);
            }
        }

        private void PopulateItems_Complete() {
            dgItems.ItemsSource = items;
            this.IsEnabled = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Edit_Click(object sender, RoutedEventArgs e) {
            var button = (Button)sender;
            var model = button.DataContext as ItemModel;
            var row = dgItems.SelectedItem as ItemModel;

            EditItem dialog = new EditItem(mainWindow, model, this);
            dialog.ShowDialog();
        }
	}
}