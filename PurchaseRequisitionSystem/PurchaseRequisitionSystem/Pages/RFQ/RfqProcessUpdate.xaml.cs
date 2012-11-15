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
	/// Interaction logic for RfqProcessUpdate.xaml
	/// </summary>
	public partial class RfqProcessUpdate : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;
        private IEnumerable<RfqItemModel> rfqItems;
        private AsyncWork async;
        public Guid rfqId;
        private Guid requestId;

		public RfqProcessUpdate()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public RfqProcessUpdate(MainWindow mainWindow, Guid rfqId,Guid requestId) : this(){
            this.mainWindow = mainWindow;
            this.requestId = requestId;
            this.rfqId = rfqId;
            async = new AsyncWork(mainWindow);
        }
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            var rfqItems = (from mri in db.mRfqs_mItemsRequests
                            join r in db.RFQs on mri.RfqId equals r.Id
                            join mi in db.mItemsRequests on mri.mItemsRequestId equals mi.Id
                            join i in db.Items on mi.ItemId equals i.Id
                            where r.Id == rfqId
                            select new { mri,mi,i });
            var selectRfqItems = rfqItems.Select(x => new RfqItemModel()
            {
                Id = x.mri.Id,
                Description = x.i.Description + ", " + x.i.ItemBrand.Name + ", " + x.i.CodeSize,
                Quantity = x.mi.Quantity,
                Discount = x.mri.Discount.HasValue ? x.mri.Discount.Value : 0,
                Price = x.mri.UnitPrice.HasValue ? x.mri.UnitPrice.Value : 0,
                TotalPrice = x.mri.TotalPrice.HasValue ? x.mri.TotalPrice.Value : 0
            });
            lstRfq.ItemsSource = selectRfqItems;
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            rfqItems = lstRfq.Items.Cast<RfqItemModel>();

            async.Do(SaveRfqItems, SaveRfqItems_Complete);
        }

        private void SaveRfqItems() {
            var CurrentRfq = db.RFQs.Where(x => x.Id == rfqId).SingleOrDefault();
            CurrentRfq.IsQuoted = true;

            foreach (var item in rfqItems) {
                var rfqItem = db.mRfqs_mItemsRequests.Where(x => x.Id == item.Id).SingleOrDefault();
                rfqItem.Discount = item.Discount;
                rfqItem.UnitPrice = item.Price;
                rfqItem.TotalPrice = (item.Price * item.Quantity) - item.Discount;
                db.SaveChanges();
            }
        }

        private void SaveRfqItems_Complete() {
            MessageBox.Show("RFQ has been updated.", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            mainWindow.TFrame.ShowPage(new RfqProcessUpdate(mainWindow,rfqId, requestId));
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new RfqSelectForUpdate(mainWindow, requestId));
        }
	}
}