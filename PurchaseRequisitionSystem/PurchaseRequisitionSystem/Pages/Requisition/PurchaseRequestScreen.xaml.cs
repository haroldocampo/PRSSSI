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
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.Background;
using System.Threading;
using PurchaseRequisitionSystem.Models;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for PurchaseRequestScreen.xaml
	/// </summary>
	public partial class PurchaseRequestScreen : UserControl
	{
        private MainWindow mainWindow;
        private AsyncWork submitRequest;
        private PrsEntities db;
        private UserModel User;

        string DateRequired;
        string Purpose;
        bool PRType;
        private IEnumerable<ItemModel> purchaseItems;

        public Guid SelectedItemId { get; set; }

		public PurchaseRequestScreen()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            User = (UserModel)App.Current.Properties["User"];
		}

        public PurchaseRequestScreen(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
            submitRequest = new AsyncWork(mainWindow);
        }

        private void btnAddNewItem_Click(object sender, RoutedEventArgs e) {
            AddItem dialog = new AddItem(mainWindow,this);
            dialog.ShowDialog();
        }

        private void btnShowSelect_Click(object sender, RoutedEventArgs e) {
            Items itemDialog = new Items(mainWindow, this);
            itemDialog.ShowDialog();
        }

        private void btnSubmitRequest_Click(object sender, RoutedEventArgs e) {
            bool Validated = false; 
            ValidateForm(txtPurpose, txtDateRequired,comboPRType,dgItems, out Validated);

            if (!Validated) {
                MessageBox.Show("Please correct the highlighted fields","Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            PrepareForInsert();
            submitRequest.Do(InsertRequest, RequestComplete);
        }

        private void PrepareForInsert() {
            var prType = (ComboBoxItem)comboPRType.SelectedItem;
            DateRequired = txtDateRequired.Text;
            Purpose = txtPurpose.Text;
            PRType = prType.Uid == "1" ? true : false;
            purchaseItems = dgItems.Items.OfType<ItemModel>();
        }

        private void InsertRequest()
        {
            var NewRequest = db.PurchaseRequests.CreateObject();
            NewRequest.Id = Guid.NewGuid();
            NewRequest.created_at = DateTime.Now;
            NewRequest.RequestedBy = User.Id;
            NewRequest.DateRequired = DateTime.Parse(DateRequired).Date;
            NewRequest.Purpose = Purpose;
            NewRequest.PRType_IsStock = PRType;
            NewRequest.Status = "For Approval of Plant Manager";
            NewRequest.IsEditable = true;
            db.PurchaseRequests.AddObject(NewRequest);

            //Insert Items
            foreach (var item in purchaseItems) {
                var RequestItem = db.mItemsRequests.CreateObject();
                RequestItem.ItemId = item.Id;
                RequestItem.RequestId = NewRequest.Id;
                RequestItem.Quantity = item.Quantity;
                RequestItem.CostObjectiveId = item.CostObjectiveId;
                RequestItem.StockOnHand = item.StockOnHand;
                if(item.Price.HasValue){
                    RequestItem.Price = item.Price.Value * item.Quantity;
                }
                db.mItemsRequests.AddObject(RequestItem);
            }

            db.SaveChanges();
            //Get PR Number Model
            NewRequest.PRNoModel = User.CompanyId + "" + NewRequest.PRNo;
            //Save Again
            db.SaveChanges();
        }

        private void RequestComplete() {
            mainWindow.TFrame.ShowPage(new PurchaseRequestScreen(mainWindow));
            mainWindow.stryLoading.Stop();
            MessageBox.Show("Operation Successful!");
        }

        private void ValidateForm(TextBox txtPurpose, DatePicker txtDateRequired, ComboBox comboPRType, DataGrid dgItems, out bool Validated) {
            Validated = false;

            if (dgItems.Items.Count <= 0) {
                dgItems.FlagAsError();
                return;
            } dgItems.FlagAsNormal();

            if (txtPurpose.Text.IsNullOrEmpty()) {
                txtPurpose.FlagAsError();
                return;
            } txtPurpose.FlagAsNormal();

            if (comboPRType.SelectedIndex == -1) {
                comboPRType.FlagAsError();
                return;
            } comboPRType.FlagAsNormal();

            if (txtDateRequired.Text.IsNullOrEmpty()) {
                txtDateRequired.FlagAsError();
                return;
            } txtDateRequired.FlagAsNormal();

            Validated = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            //Populate
        }
	}
}