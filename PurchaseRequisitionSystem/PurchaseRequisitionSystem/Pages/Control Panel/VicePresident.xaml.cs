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
using PurchaseRequisitionSystem.Models;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for VicePresident.xaml
	/// </summary>
	public partial class VicePresident : UserControl
	{
        private MainWindow mainWindow;
        private UserModel User;

		public VicePresident()
		{
			this.InitializeComponent();
            User = (UserModel)App.Current.Properties["User"];
		}

        public VicePresident(MainWindow mainWindow)
            : this() {
            this.mainWindow = mainWindow;
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e) {
            App.Current.Properties["User"] = null;

            ImageBrush image = new ImageBrush();
            image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Purchase Requisition System;component/Content/Login.png"));
            mainWindow.btnHomeLogin.Background = image;
            mainWindow.btnHomeLogin.Content = "Login";

            mainWindow.btnMyRequests.IsEnabled = false;
            mainWindow.btnPurchaseRequest.IsEnabled = false;

            mainWindow.TFrame.ShowPage(new Login(mainWindow));
        }

        private void btnApproveRequests_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new ApprovalRequest(mainWindow));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            tbUsername.Text = User.FirstName;
        }

        private void btnCanvasSheet_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new CsApprove(mainWindow));
        }

        private void btnPurchaseOrders_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new PoSelectRequest(mainWindow));
        }

        private void btnViewRequests_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new AllRequests(mainWindow));
        }
	}
}