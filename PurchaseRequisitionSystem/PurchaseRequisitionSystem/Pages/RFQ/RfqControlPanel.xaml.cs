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

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for RfqControlPanel.xaml
	/// </summary>
	public partial class RfqControlPanel : UserControl
	{
        private MainWindow mainWindow;

		public RfqControlPanel()
		{
			this.InitializeComponent();
		}

        public RfqControlPanel(MainWindow mainWindow) : this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new RfqSelectRequest(mainWindow));
        }

        //private void btnUpdate_Click(object sender, RoutedEventArgs e) {
        //    mainWindow.TFrame.ShowPage(new RfqSelectForUpdate(mainWindow));
        //}
	}
}