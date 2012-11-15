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

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for PoSelectRequest.xaml
	/// </summary>
	public partial class PoSelectRequest : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;

		public PoSelectRequest()
		{
            this.InitializeComponent();
            db = new PrsEntities();
            this.Loaded +=new RoutedEventHandler(PoSelectRequest_Loaded);
		}

        public PoSelectRequest(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
        }

        void PoSelectRequest_Loaded(object sender, RoutedEventArgs e) {
            var requests = db.PurchaseRequests.Where(x => x.IsPoReady == true && x.IsFinished == false).Select( x=> new RequestModel(){
                Id = x.Id,
                PRNumber = x.PRNo,
                Purpose = x.Purpose,
                PRType = x.PRType_IsStock.Value ? "Stock" : "Non-Stock",
                RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                DateRequired = x.DateRequired,
                Status = x.Status
            });
            dgRequests.ItemsSource = requests;
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a purchase request first.");
                return;
            }

            var model = (RequestModel)dgRequests.SelectedItem;
            PoGenerateWindow PoWindow = new PoGenerateWindow(mainWindow, model.Id);
            PoWindow.ShowDialog();
        }

	}
}