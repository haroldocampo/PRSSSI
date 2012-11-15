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
using PurchaseRequisitionSystem.Models;
using System.Linq;
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for BranchManagement.xaml
	/// </summary>
	public partial class BranchManagement : UserControl
	{
        private MainWindow mainWindow;
        PrsEntities db;
        private AsyncWork asyncLoad;
        private IQueryable<DepAndBranchModel> ListBranches;
        private IQueryable<CompanyModel> ListCompanies;

		public BranchManagement()
		{
			this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(BranchManagement_Loaded);
            db = new PrsEntities();
		}

        public BranchManagement(MainWindow mainWindow) :this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            asyncLoad = new AsyncWork(mainWindow);
        }

        void BranchManagement_Loaded(object sender, RoutedEventArgs e) {
            asyncLoad.Do(PopulateData, PopulateData_Complete);
        }

        private void PopulateData() {
            ListBranches = db.Branches.Select(x => new DepAndBranchModel() { 
                Id = x.Id,
                Name = x.Name,
                Company = x.Company.Name
            });

            ListCompanies = db.Companies.Select(x => new CompanyModel() { 
                Id = x.Id,
                Name = x.Name
            });
        }

        private void PopulateData_Complete() {
            lstData.ItemsSource = ListBranches;
            cbCompany.ItemsSource = ListCompanies;
            cbCompany.IsEnabled = true;
        }

        private void lstData_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var DataModel = lstData.SelectedItem as DepAndBranchModel;
            txtName.Text = DataModel.Name;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            //Validations
            if (cbCompany.SelectedIndex == -1) {
                cbCompany.FlagAsError();
                return;
            } cbCompany.FlagAsNormal();

            if (txtName.Text.IsNullOrEmpty()) {
                txtName.FlagAsError();
                return;
            } txtName.FlagAsNormal();

            if (ListBranches.Any(x => x.Name == txtName.Text && x.Company == cbCompany.Text)) {
                MessageBox.Show("There is a same record of this already.");
                return;
            }
            //End of Validations

            var CompanyModel = cbCompany.SelectedItem as CompanyModel;

            //Add and Save Data
            var newEntity = db.Branches.CreateObject();
            newEntity.CompanyId = CompanyModel.Id;
            newEntity.Name = txtName.Text;
            db.Branches.AddObject(newEntity);
            db.SaveChanges();
            // Refresh Page
            mainWindow.TFrame.ShowPage(new BranchManagement(mainWindow));

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            //Validations
            if (cbCompany.SelectedIndex == -1) {
                cbCompany.FlagAsError();
                return;
            } cbCompany.FlagAsNormal();

            if (lstData.SelectedIndex == -1) {
                lstData.FlagAsError();
                return;
            } lstData.FlagAsNormal();

            if (txtName.Text.IsNullOrEmpty()) {
                txtName.FlagAsError();
                return;
            } txtName.FlagAsNormal();
            //End of Validations

            var CompanyModel = cbCompany.SelectedItem as CompanyModel;
            var DataModel = lstData.SelectedItem as DepAndBranchModel;

            //Edit and Save Data
            var selectedEntity = db.Branches.Where(x => x.Id == DataModel.Id).SingleOrDefault();
            selectedEntity.CompanyId = CompanyModel.Id;
            selectedEntity.Name = txtName.Text;
            db.SaveChanges();
            // Refresh page
            mainWindow.TFrame.ShowPage(new BranchManagement(mainWindow));
        }
	}
}