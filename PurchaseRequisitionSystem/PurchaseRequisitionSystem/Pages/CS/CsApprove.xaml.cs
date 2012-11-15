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
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for CsApprove.xaml
	/// </summary>
	public partial class CsApprove : UserControl
	{
        private MainWindow mainWindow;
        private AsyncWork async,async2,async3;
        private PrsEntities db;
        private CanvassRequestModel model;
        private UserModel User;

		public CsApprove()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            User = (UserModel)App.Current.Properties["User"];
		}

        public CsApprove(MainWindow mainWindow) :this() {
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow); async3 = new AsyncWork(mainWindow);
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e) {
            if (User.UserTypeId == (int)UserTypes.VicePresident)
                mainWindow.TFrame.ShowPage(new VicePresident(mainWindow));
            else { // User Type will be purchasing manager
                mainWindow.TFrame.ShowPage(new Purchasing(mainWindow));
            }
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            if(dgRequests.SelectedIndex == -1){
                MessageBox.Show("Please select a request to continue.", "Oops!", MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            model = (CanvassRequestModel)dgRequests.SelectedItem;
            CsApproveWindow csWindow = new CsApproveWindow(mainWindow, model.CanvasId,model.RequestId);
            csWindow.ShowDialog();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            dgRequests.ItemsSource = (from cs in db.CanvasSheets
                                      join r in db.PurchaseRequests on cs.RequestId equals r.Id
                                      where cs.is_approved == false
                                      select new { r,cs }).Select(x =>
                    new CanvassRequestModel
                    {
                        RequestId = x.r.Id,
                        PRNumber = x.r.PRNo,
                        Purpose = x.r.Purpose,
                        RequestedBy = x.r.User.LastName + ", " + x.r.User.FirstName,
                        DateRequired = x.r.DateRequired,
                        Status = x.r.Status,
                        CanvasId = x.cs.Id,
                        CSNumber = x.cs.CsNumber
                    });
        }
	}
}