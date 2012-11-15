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
using Infrastructure.Helpers.Extensions;
using System.Linq;
using System.Data;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for SelectItem.xaml
	/// </summary>
	public partial class Items : Window
	{
        private PurchaseRequestScreen requestScreen;
        private PrsEntities db;
        public int CostObjectiveId;
        private MainWindow mainWindow;
        private IQueryable<ItemModel> allItems;
        private EditRequisition editRequisition;

		public Items()
		{
			this.InitializeComponent();
            db = new PrsEntities();
        }

        public Items(MainWindow mainWindow, PurchaseRequestScreen purchaseRequestScreen):this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.requestScreen = purchaseRequestScreen;
        }

        public Items(MainWindow mainWindow, EditRequisition editRequisition) :this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.editRequisition = editRequisition;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            allItems = db.Items.Where(x=>x.is_active==true).Select(x => new ItemModel { Id = x.Id,ItemNumber =x.ItemNumber , Price=x.Price, Description = x.Description, Brand = x.ItemBrand.Name, CodeSize = x.CodeSize, UOM = x.UnitOfMeasurement.Name});
            lstItems.ItemsSource = allItems;
            var uoms = db.UnitOfMeasurements.Select(x => new TmpUOM() { Id=x.Id, Name=x.Name });
            cbUnitOfMeasurements.ItemsSource = uoms;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e) {
            if (lstItems.SelectedIndex == -1) {
                MessageBox.Show("Select an Item", "Oops!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CostObjectiveId == 0) {
                MessageBox.Show("Please select a cost objective.");
                return;
            }

            if (cbUnitOfMeasurements.SelectedIndex == -1) {
                MessageBox.Show("Please select a Unit of Measurement.");
                return;
            }

            var item = (ItemModel)lstItems.SelectedItem;
            Guid itemId = item.Id;
            item.Quantity = txtQuantity.Text.ToInt();
            item.StockOnHand = txtStockOnHand.Text.ToInt();
            item.CostObjectiveId = CostObjectiveId;
            item.CostObjective = tbCostObjectiveName.Text;

            // If in Request Screen
            if (requestScreen != null) {
                IEnumerable<ItemModel> enumItems = requestScreen.dgItems.Items.Cast<ItemModel>();

                //Iterate Items and check for duplicates
                bool IsExisting = false;
                foreach (var tmpItem in enumItems) {
                    // Check if Item Id is the same
                    if (itemId == tmpItem.Id) {
                        IsExisting = true;
                        break;
                    }
                }

                if (IsExisting) {
                    MessageBox.Show("Item already exists in the list");
                    return;
                }


                if (requestScreen.dgItems.Items.Count >= 10) {
                    MessageBox.Show("You can only have a maximum of 10 items");
                    return;
                }

                requestScreen.dgItems.Items.Add(item);
            }

            // Change unit of measurement
            var tmpItemForUom = db.Items.Where(x => x.Id == itemId).SingleOrDefault();
            var uom = cbUnitOfMeasurements.SelectedItem as TmpUOM;
            tmpItemForUom.ItemUOMId = uom.Id;
            // Insert UOM name
            item.UOM = uom.Name;
            db.SaveChanges();

            // If in Edit Requisition Window
            if (editRequisition != null) {
                editRequisition.AddItemRequest(item);
            }

            this.Close();
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e) {
            txtQuantity.ForceNumeric();
        }

        private void lstItems_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void txtStockOnHand_TextChanged(object sender, TextChangedEventArgs e) {
            txtStockOnHand.ForceNumeric();
        }

        private void btnCostObjSelect_Click(object sender, RoutedEventArgs e) {
            SelectCostObjective selectDialog = new SelectCostObjective(mainWindow, this);
            selectDialog.ShowDialog();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            var items = allItems.Where(x=>x.Description.Contains(txtSearch.Text));
            lstItems.ItemsSource = items;
        }
	}

    public class TmpUOM {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString() {
            return Name;
        }
    }

    public class ItemModel {
        public Guid Id { get; set; }
        public int ItemNumber { get; set; }
        public int StockOnHand { get; set; }
        public int CostObjectiveId { get; set; }
        public int ext_id { get; set; }
        public int Quantity { get; set; }
        public string UOM { get; set; }
        public double? Price { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string CodeSize { get; set; }
        public string Brand { get; set; }
        public string CostObjective { get; set; }
        public bool EnableEdit { get; set; }
        public bool IsNewItem { get; set; }
    }
}