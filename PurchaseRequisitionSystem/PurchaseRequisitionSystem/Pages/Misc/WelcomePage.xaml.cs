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
using System.Windows.Media.Animation;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for WelcomePage.xaml
	/// </summary>
	public partial class WelcomePage : UserControl
	{
        public Storyboard stryWelcome;
        private MainWindow mainWindow;

        public WelcomePage() {
            this.InitializeComponent();
        }

        public WelcomePage(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            stryWelcome = (Storyboard)FindResource("stryWelcome");
            stryWelcome.Completed += new EventHandler(stryWelcome_Completed);
        }

        void stryWelcome_Completed(object sender, EventArgs e) {
            mainWindow.TFrame.ShowPage(new Login(mainWindow));
        }
	}
}