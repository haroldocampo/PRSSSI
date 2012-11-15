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
	/// Interaction logic for ItemManagement.xaml
	/// </summary>
	public partial class ItemManagement : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;

		public ItemManagement()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public ItemManagement(MainWindow mainWindow) : this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
        }

        private void btnUOM_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new DataManagement(mainWindow, new UnitOfMeasurement().GetType()));
        }

        private void btnItemBrand_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new DataManagement(mainWindow, new ItemBrand().GetType()));
        }

        private void btnCostObjective_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new CostObjectiveManagement(mainWindow));
        }

        private void btnAddVendor_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new AddVendor(mainWindow));
        }

        private void btnGenerateReports_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new Reports(mainWindow));
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            AddItem dialog = new AddItem(mainWindow);
            dialog.ShowDialog();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new Register(mainWindow));
        }

        private void btnDepartments_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new DepartmentManagement(mainWindow));
        }

        private void btnBranches_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new BranchManagement(mainWindow));
        }
	}
}