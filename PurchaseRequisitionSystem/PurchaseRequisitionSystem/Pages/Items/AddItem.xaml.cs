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
using System.ComponentModel;
using PurchaseRequisitionSystem.Background;
using Infrastructure;
using Infrastructure.Helpers;
using Infrastructure.Helpers.Extensions;
using Infrastructure.SpecialObjects;
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for AddItem.xaml
	/// </summary>
	public partial class AddItem : Window
	{
        //Private Variables
        private MainWindow mainWindow;
        private AsyncWork background;
        private PrsEntities db;
        private UserModel User;

        //The item to insert
        private Item item;

        //UI Variables
        string Description;
        string CodeSize = "";
        int UOM;
        int Brand;
        int CostObjectiveId;

        //List Variables
        private List<object> brands;
        private List<object> uoms;

        bool isRequestItemOnly = true;
        public int SelectedCostObjective;

        bool IsRequesting = true; 
        private PurchaseRequestScreen requestScreen;
        private KeyAndValue modelUOM;
        private KeyAndValue modelBrand;
        private EditRequisition editRequisition;
        public AddItem()
		{
			this.InitializeComponent();
           
            db = new PrsEntities();
            User = App.Current.Properties["User"] as UserModel;
            if (User.UserTypeId == (int)UserTypes.Purchasing) {
                isRequestItemOnly = false;
            }
		}

        public AddItem(MainWindow mainWindow) : this() {
            IsRequesting = false;
            stackQuantity.Visibility = Visibility.Hidden;
            this.mainWindow = mainWindow;
            background = new AsyncWork(mainWindow);
        }

        public AddItem(MainWindow mainWindow, PurchaseRequestScreen purchaseRequestScreen)
            : this() {
            this.mainWindow = mainWindow;
            this.requestScreen = purchaseRequestScreen;

            background = new AsyncWork(mainWindow);
        }

        public AddItem(MainWindow mainWindow, EditRequisition editRequisition) : this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.editRequisition = editRequisition;

            background = new AsyncWork(mainWindow);
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            modelUOM = (KeyAndValue)cbUnitOfMeasurement.SelectedItem;
            modelBrand = (KeyAndValue)cbBrand.SelectedItem;

            bool IsValid = false;

            //Cost Objective is Always Mandatory Select Cost objective
            CostObjectiveId = SelectedCostObjective == 0 ? 0 : SelectedCostObjective;
            if (CostObjectiveId == 0) {
                btnCostObjSelect.FlagAsError();
                MessageBox.Show("Cost Objective is Mandatory.", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                btnCostObjSelect.FlagAsError();
                return;
            } btnCostObjSelect.FlagAsCorrect();

            if (!isRequestItemOnly) {
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
            }
            //Prepare for Addition and Validate Textboxes
            PrepareForInsert(modelUOM, modelBrand, out IsValid);

            if (!IsValid) {
                MessageBox.Show("Please correct the highlighted boxes");
                return;
            }

            //Add Item
            background.Do(AddNewItem, AddNewItem_Complete);
        }

        private void PrepareForInsert(KeyAndValue modelUOM, KeyAndValue modelBrand, out bool isValid) {
            CodeSize = txtCodeSize.Text;
            Description = txtDescription.Text;
            // IDs containt Generic values in the database if request only
            UOM = modelUOM == null ? 134 : (int)modelUOM.Key;
            Brand = modelBrand == null ? 7 : (int)modelBrand.Key;

            if (!isRequestItemOnly) {
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
            }
            isValid = true;
        }

        public void AddNewItem() {
            item = db.Items.CreateObject();
            item.Id = Guid.NewGuid();
            item.Description = Description;

            item.ItemUOMId = UOM;
            item.CodeSize = this.CodeSize;
            item.ItemBrandId = Brand;

            //TODO: Improve
            if(User.UserTypeId == (int)UserTypes.Purchasing)
                item.is_active = true;
            else
                item.is_active = false;

            item.created_by = User.Id;
            item.date_created = DateTime.Now;   
            db.AddToItems(item);
            db.SaveChanges();
        }

        public void AddNewItem_Complete() {
            string brandText = "Generic"; string uomText = "Generic";
            if (modelBrand != null) {
                brandText = modelBrand.Value;
            }
            if (modelUOM != null) {
                uomText = modelUOM.Value;
            }
            if (IsRequesting) {
                ItemModel newItem = new ItemModel()
                {
                    Id = item.Id,
                    ItemNumber = item.ItemNumber,
                    Description = item.Description,
                    Quantity = txtQuantity.Text.ToInt(),
                    Brand = brandText,
                    CodeSize = CodeSize,
                    UOM = uomText,
                    CostObjectiveId = this.CostObjectiveId,
                    CostObjective = tbCostObjectiveName.Text,
                    StockOnHand = txtStockOnHand.Text.ToInt()
                };
                //Add an item
                if (requestScreen != null) {
                    requestScreen.dgItems.Items.Add(newItem);
                }

                if (editRequisition != null) {
                    editRequisition.AddItemRequest(newItem);
                }
            }
            else {
                MessageBox.Show("Item has been successfully added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            this.Close();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            background.Do(PopulateData, PopulateDataComplete);
        }

        private void PopulateData() {
            brands = new List<object>();
            uoms = new List<object>();

            foreach (var uom in db.UnitOfMeasurements) {
                KeyAndValue tmpUOM = new KeyAndValue(uom.Id, uom.Name);
                uoms.Add(tmpUOM);
            }

            foreach (var brand in db.ItemBrands) {
                KeyAndValue tmpBrand = new KeyAndValue(brand.Id, brand.Name);
                brands.Add(tmpBrand);
            }
        }

        private void PopulateDataComplete() {
            cbBrand.ItemsSource = brands;
            cbUnitOfMeasurement.ItemsSource = uoms;
        }

        private void btnCostObjSelect_Click(object sender, RoutedEventArgs e) {
            SelectCostObjective selectDialog = new SelectCostObjective(mainWindow,this);
            selectDialog.ShowDialog();
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e) {
            txtQuantity.ForceNumeric();
        }

        private void txtStockOnHand_TextChanged(object sender, TextChangedEventArgs e) {
            txtStockOnHand.ForceNumeric();
        }
    }
}