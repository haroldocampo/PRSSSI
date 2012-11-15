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
using Infrastructure.SpecialObjects;
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for EditItem.xaml
	/// </summary>
	public partial class EditItem : Window
	{
        ItemModel Item;
        public int SelectedCostObjective;

        //UI Variables
        string Description;
        string CodeSize;
        int UOM;
        int Brand;
        int CostObjectiveId;
        int Quantity;

        //List Variables
        private List<object> brands;
        private List<object> uoms;

        private MainWindow mainWindow;
        private ViewRequest viewDialog;
        private PrsEntities db;
        private AsyncWork background;

        public EditItem(MainWindow mainWindow, ItemModel item) {
            this.InitializeComponent();
            this.Item = item;
            this.mainWindow = mainWindow;
            db = new PrsEntities();
            background = new AsyncWork(mainWindow);
            this.Loaded += new RoutedEventHandler(EditItem_Loaded);
        }

		public EditItem(MainWindow mainWindow, ItemModel item, ViewRequest viewDialog) : this(mainWindow, item)
		{
            this.viewDialog = viewDialog;
		}

        public EditItem(MainWindow mainWindow, ItemModel model, EditRequisition editRequisition) : this (mainWindow, model){
            this.editRequisition = editRequisition;
            stackBrand.Visibility = Visibility.Collapsed;
            stackCodeSize.Visibility = Visibility.Collapsed;
            stackDescription.Visibility = Visibility.Collapsed;
            stackUOM.Visibility = Visibility.Collapsed;
        }

        void EditItem_Loaded(object sender, RoutedEventArgs e) {
            var itemEntity = db.Items.Where(x => x.Id == Item.Id).SingleOrDefault();
            UOM = itemEntity.ItemUOMId.HasValue ? itemEntity.ItemUOMId.Value : 0;
            Brand = itemEntity.ItemBrandId.HasValue ? itemEntity.ItemBrandId.Value : 0;

            txtDescription.Text = Item.Description;
            txtCodeSize.Text = Item.CodeSize;
            tbCostObjectiveName.Text = Item.CostObjective;
            SelectedCostObjective = Item.CostObjectiveId;
            txtQuantity.Text = Item.Quantity.ToString();

            background.Do(PopulateData, PopulateDataComplete);
        }

        private void PopulateData() {
            brands = new List<object>();
            uoms = new List<object>();

            foreach (var uom in db.UnitOfMeasurements) {
                KeyAndValue tmpUOM = new KeyAndValue(uom.Id, uom.Name);
                uoms.Add(tmpUOM);

                if (UOM != 0 && (int)tmpUOM.Key == UOM)
                    selectedUOM = tmpUOM;
            }

            foreach (var brand in db.ItemBrands) {
                KeyAndValue tmpBrand = new KeyAndValue(brand.Id, brand.Name);
                brands.Add(tmpBrand);

                if (UOM != 0 && (int)tmpBrand.Key == Brand)
                    selectedBrand = tmpBrand;
            }
        }
        KeyAndValue selectedUOM; KeyAndValue selectedBrand;
        private EditRequisition editRequisition;
        private void PopulateDataComplete() {
            cbBrand.ItemsSource = brands;
            cbUnitOfMeasurement.ItemsSource = uoms;

            if (UOM != 0)
                cbUnitOfMeasurement.SelectedItem = selectedUOM;
            if (Brand != 0)
                cbBrand.SelectedItem = selectedBrand;
        }

        private void btnCostObjSelect_Click(object sender, RoutedEventArgs e) {
            SelectCostObjective selectDialog = new SelectCostObjective(mainWindow, this);
            selectDialog.ShowDialog();
        }

        private void btnEditItem_Click(object sender, RoutedEventArgs e) {
            var modelUOM = (KeyAndValue)cbUnitOfMeasurement.SelectedItem;
            var modelBrand = (KeyAndValue)cbBrand.SelectedItem;

            bool IsValid = false;

            #region ComboBox Validations
            //Validate ComboBoxes
            if (cbUnitOfMeasurement.SelectedIndex == -1) {
                cbUnitOfMeasurement.FlagAsError();
                return;
            } cbUnitOfMeasurement.FlagAsCorrect();

            if (cbBrand.SelectedIndex == -1) {
                cbBrand.FlagAsError();
                return;
            } cbBrand.FlagAsCorrect();

            #endregion
            
            //Prepare for Addition and Validate Textboxes
            PrepareForEdit(modelUOM, modelBrand, out IsValid);

            if (!IsValid) {
                MessageBox.Show("Please correct the highlighted boxes");
                return;
            }

            //Add Item
            background.Do(EditNewItem, EditNewItem_Complete);
        }

        private void PrepareForEdit(KeyAndValue modelUOM, KeyAndValue modelBrand, out bool isValid) {
            Description = txtDescription.Text;
            CodeSize = txtCodeSize.Text;
            Quantity = txtQuantity.Text.ToInt();
            // IDs containt Generic values in the 4database if request only
            UOM = modelUOM == null ? 134 : (int)modelUOM.Key;
            Brand = modelBrand == null ? 7 : (int)modelBrand.Key;
            CostObjectiveId = SelectedCostObjective == 0 ? 0 : SelectedCostObjective;

            if (CostObjectiveId == 0) {
                btnCostObjSelect.FlagAsError();
                isValid = false;
                return;
            } btnCostObjSelect.FlagAsCorrect();

            if (Description.IsNullOrEmpty()) {
                txtDescription.FlagAsError();
                isValid = false;
                return;
            } txtDescription.FlagAsCorrect();
            
            isValid = true;
        }

        public void EditNewItem() {
            var item = db.Items.Where(x => x.Id == Item.Id).SingleOrDefault();
            item.Description = Description;

            item.ItemUOMId = UOM;
            item.CodeSize = CodeSize;
            item.ItemBrandId = Brand;
            item.is_active = true;

            var itemRequest = db.mItemsRequests.Where(x => x.Id == Item.ext_id).SingleOrDefault();
            itemRequest.CostObjectiveId = CostObjectiveId;
            itemRequest.Quantity = Quantity;

            db.SaveChanges();
        }

        public void EditNewItem_Complete() {
            MessageBox.Show("The Item has been updated.\n You may now proceed again.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            if (viewDialog != null) {
                viewDialog.Close();
            }

            if (editRequisition != null) {
                Item.CostObjective = tbCostObjectiveName.Text;
                Item.Quantity = Quantity;
                mainWindow.TFrame.ShowPage(new EditRequisition(mainWindow,editRequisition.RequestId));
            }

            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e) {
            txtQuantity.ForceNumeric();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
	}
}