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
	/// Interaction logic for OverallHead.xaml
	/// </summary>
	public partial class OverallHead : UserControl
	{
        private MainWindow mainWindow;
        private UserModel User;

		public OverallHead()
		{
			this.InitializeComponent();
            User = (UserModel)App.Current.Properties["User"];
		}

        public OverallHead(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            tbUsername.Text = User.FirstName;
        }

        private void btnApprove_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new ApprovalRequest(mainWindow));
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

        private void btnViewAllRequests_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.TFrame.ShowPage(new AllRequests(mainWindow));
        }
	}
}