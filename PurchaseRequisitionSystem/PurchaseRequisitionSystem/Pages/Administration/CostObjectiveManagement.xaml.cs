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
using Infrastructure.SpecialObjects;
using System.Collections;
using PurchaseRequisitionSystem.Background;
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.Models;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for CostObjectiveManagement.xaml
	/// </summary>
	public partial class CostObjectiveManagement : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;

        private AsyncWork async, async2, async3,async4;

        private List<object> companies;
        private List<object> categories;
        private IEnumerable CostObjectives;

        private CostObjectiveModel SelectedData;

        string _txtName;
        string _txtSearch;

        int _categoryId;
        int _companyId;

		public CostObjectiveManagement()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public CostObjectiveManagement(MainWindow mainWindow) :this() {
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow); async3 = new AsyncWork(mainWindow); async4 = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            async.Do(PopulateData, PopulateDataComplete);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (cbCompany.SelectedIndex == -1) {
                cbCompany.FlagAsError();
                return;
            } cbCompany.FlagAsNormal();

            if (cbCategory.SelectedIndex == -1) {
                cbCategory.FlagAsError();
                return;
            } cbCategory.FlagAsNormal();

            var category = (KeyAndValue)cbCategory.SelectedItem;
            var company = (KeyAndValue)cbCompany.SelectedItem;
            _categoryId = (int)category.Key;
            _companyId = (int)company.Key;
            _txtName = txtName.Text;

            // Validate Textbox
            if (_txtName.IsNullOrEmpty()) {
                txtName.FlagAsError();
                return;
            } txtName.FlagAsNormal();

            async2.Do(AddData, OnAsyncAddUpdateComplete);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            if (cbCompany.SelectedIndex == -1) {
                cbCompany.FlagAsError();
                return;
            } cbCompany.FlagAsNormal();

            if (cbCategory.SelectedIndex == -1) {
                cbCategory.FlagAsError();
                return;
            } cbCategory.FlagAsNormal();

            var category = (KeyAndValue)cbCategory.SelectedItem;
            var company = (KeyAndValue)cbCompany.SelectedItem;
            _categoryId = (int)category.Key;
            _companyId = (int)company.Key;
            _txtName = txtName.Text;

            //Validate Selection
            if (lstData.SelectedIndex == -1) { //If none selected then..
                lstData.FlagAsError();
                return;
            } lstData.FlagAsNormal();

            // Validate Textbox
            if (_txtName.IsNullOrEmpty()) {
                txtName.FlagAsError();
                return;
            } txtName.FlagAsNormal();

            async3.Do(UpdateData, OnAsyncAddUpdateComplete);
        }

        private void AddData() {
            db.ExecuteStoreCommand("Insert into {0} (Name,CategoryId,CompanyId) values ('{1}',{2},{3})".WithTokens("CostObjectives",_txtName, _categoryId, _companyId), null);
        }

        private void UpdateData() {
            string command = "Update {0} SET Name='{1}', CategoryId = {2},CompanyId= {3} WHERE Id={4}".WithTokens("CostObjectives", _txtName, _categoryId, _companyId, SelectedData.Id);
            db.ExecuteStoreCommand(command, null);
        }

        private void OnAsyncAddUpdateComplete() {
            mainWindow.TFrame.ShowPage(new CostObjectiveManagement(mainWindow));
        }

        private void PopulateData() {
            companies = new List<object>();
            categories = new List<object>();

            CostObjectives = db.CostObjectives.Select(x => new CostObjectiveModel { Id = x.Id, Name = x.Name, Category = x.CostObjectiveCategory.Name, Company = x.Company.Name });

            foreach (var company in db.Companies) {
                KeyAndValue tmpCompany = new KeyAndValue(company.Id, company.Name);
                companies.Add(tmpCompany);
            }

            foreach (var type in db.CostObjectiveCategories) {
                KeyAndValue tmpType = new KeyAndValue(type.Id, type.Name);
                categories.Add(tmpType);
            }
        }

        private void PopulateDataComplete() {
            lstData.ItemsSource = CostObjectives;
            lstData.ClearFirstColumn();
            cbCategory.ItemsSource = categories;
            cbCompany.ItemsSource = companies;
            cbCompany.IsEnabled = true;
            cbCategory.IsEnabled = true;
            mainWindow.stryLoading.Stop();
        }

        IEnumerable searchSource;
        private void SearchData() {
            searchSource = db.CostObjectives.Select(x => new CostObjectiveModel { Id=x.Id, Name = x.Name, Company = x.Company.Name, Category = x.CostObjectiveCategory.Name }).Where(x=>x.Name.Contains(_txtSearch));
        }

        private void OnSearchComplete() {
            lstData.ItemsSource = searchSource;
            lstData.ClearFirstColumn();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            _txtSearch = txtSearch.Text;
            async4.Do(SearchData, OnSearchComplete);
        }

        private void lstData_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {
            SelectedData = (CostObjectiveModel)lstData.SelectedItem;
        }
    }
}