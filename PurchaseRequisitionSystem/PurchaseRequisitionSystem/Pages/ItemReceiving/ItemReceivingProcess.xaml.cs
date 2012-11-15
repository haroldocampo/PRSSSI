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
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for ItemReceivingProcess.xaml
	/// </summary>
	public partial class ItemReceivingProcess : UserControl
	{
        private MainWindow mainWindow;
        private Guid RequestId;
        private PrsEntities db;
        private List<ItemReceivingModel> items { get; set; }
        private AsyncWork asyncLoad;

		public ItemReceivingProcess()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            this.Loaded += new RoutedEventHandler(ItemReceivingProcess_Loaded);
		}

        public ItemReceivingProcess(MainWindow mainWindow, Guid RequestId) :this() {
            this.mainWindow = mainWindow;
            this.RequestId = RequestId;
            asyncLoad = new AsyncWork(mainWindow);
        }

        void ItemReceivingProcess_Loaded(object sender, RoutedEventArgs e) {
            asyncLoad.Do(PopulateItemsRequests, PopulateItemsRequests_Complete);
        }

        private void PopulateItemsRequests() {
            items = new List<ItemReceivingModel>();
            IQueryable<mItemsRequest> AllItems = db.mItemsRequests;
            var itemRequests = AllItems.Where(x => x.RequestId == RequestId);
            foreach (var itemRequest in itemRequests) {
                IQueryable<Item> tmpItems = db.Items;
                var item = tmpItems.Where(x => x.Id == itemRequest.ItemId).SingleOrDefault();
                var requestItem = new ItemReceivingModel()
                {
                    Id = itemRequest.Id,
                    Description = item.Description + ", " + item.ItemBrand.Name + ", " + item.CodeSize,
                    UOM = item.UnitOfMeasurement.Name,
                    QuantityRequested = itemRequest.Quantity,
                    QuantityReceived = itemRequest.ReceivedQuantity.HasValue ? itemRequest.ReceivedQuantity.Value.ToString() : null,
                    DrInvoiceNumber = itemRequest.DrInvoiceNumber
                };
                items.Add(requestItem);
            }
        }

        private void PopulateItemsRequests_Complete() {
            lstItemReceiving.ItemsSource = items;
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new ItemReceivingSelectRequest(mainWindow));
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            items = lstItemReceiving.Items.Cast<ItemReceivingModel>().ToList();

            foreach (var item in items) {
                if(!item.QuantityReceived.IsInteger()){
                    MessageBox.Show("Please check your inputs.\nSome of them may not be valid.","Oops!",MessageBoxButton.OK,MessageBoxImage.Error);
                    return;
                }
                // Update Item Receiving in mItemsRequest table
                var model = db.mItemsRequests.Where(x => x.Id == item.Id).SingleOrDefault();
                model.ReceivedQuantity = item.QuantityReceived.ToInt();
                model.DrInvoiceNumber = item.DrInvoiceNumber;

                //Update Items Last Purchase Date
                var itemId = db.mItemsRequests.Where(x => x.Id == item.Id).SingleOrDefault().ItemId;
                var selectedItem = db.Items.Where(x => x.Id == itemId).SingleOrDefault();
                selectedItem.LastPurchasedDate = DateTime.Now.Date;
            }

            db.SaveChanges();

            // Check if items are all received
            CheckIfItemsAreCompleteAndSave();

            MessageBox.Show("The items data have been updated", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            mainWindow.TFrame.ShowPage(new ItemReceivingSelectRequest(mainWindow));
        }

        private void CheckIfItemsAreCompleteAndSave() {
            var IsItemsComplete = db.mItemsRequests.Any(x => x.RequestId == RequestId && x.ReceivedQuantity >= x.Quantity);
            if (IsItemsComplete) {
                var Request = db.PurchaseRequests.Where(x => x.Id == RequestId).SingleOrDefault();

                Request.IsFinished = true;
                Request.Status = "Complete";

                db.SaveChanges();
            }
        }
    }
}