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
	/// Interaction logic for EditRequisition.xaml
	/// </summary>
	public partial class EditRequisition : UserControl
	{
        private MainWindow mainWindow;
        public Guid RequestId;
        private PrsEntities db;
        AsyncWork asyncLoad;
        private IEnumerable<ItemModel> RequestItems;

		public EditRequisition()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public EditRequisition(MainWindow mainWindow, Guid requestId) :this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.RequestId = requestId;
            asyncLoad = new AsyncWork(mainWindow);
            asyncLoad.Do(PopulateData, PopulateData_Complete);
        }

        private void PopulateData() {
            RequestItems = db.mItemsRequests.Where(x => x.RequestId == RequestId).Select(x => new ItemModel()
            {
                Id = x.ItemId,
                CodeSize = x.Item.CodeSize,
                ext_id = x.Id,
                IsNewItem = false,
                CostObjective = x.CostObjective.Name,
                CostObjectiveId = x.CostObjectiveId,
                Description = x.Item.Description,
                Quantity = x.Quantity
            });
        }

        private void PopulateData_Complete() {
            foreach(var itemModel in RequestItems){
                lstItems.Items.Add(itemModel);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            Button button = sender as Button;
            var model = button.DataContext as ItemModel;

            EditItem editItemWindow = new EditItem(mainWindow, model, this);
            editItemWindow.ShowDialog();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            Button button = sender as Button;
            var model = button.DataContext as ItemModel;

            if (lstItems.Items.Count <= 1) {
                MessageBox.Show("Cannot delete all items. \n\nPlease add another item before deleting this one.");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No) {
                return;
            }

            var itemRequest = db.mItemsRequests.Where(x => x.Id == model.ext_id).SingleOrDefault();

            db.DeleteObject(itemRequest);

            db.SaveChanges();

            lstItems.Items.Remove(model);
        }

        private void btnSelectItem_Click(object sender, RoutedEventArgs e) {
            Items selectItemWindow = new Items(mainWindow, this);
            selectItemWindow.ShowDialog();
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            AddItem addItemWindow = new AddItem(mainWindow, this);
            addItemWindow.ShowDialog();
        }

        public void AddItemRequest(ItemModel itemModel) {
            var newItem = db.mItemsRequests.CreateObject();
            newItem.ItemId = itemModel.Id;
            newItem.CostObjectiveId = itemModel.CostObjectiveId;
            newItem.StockOnHand = itemModel.StockOnHand;
            newItem.Quantity = itemModel.Quantity;
            newItem.RequestId = this.RequestId;

            db.mItemsRequests.AddObject(newItem);

            db.SaveChanges();

            itemModel.ext_id = newItem.Id;

            lstItems.Items.Add(itemModel);
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new MyRequests(mainWindow));
        }
	}
}