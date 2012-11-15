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
using System.Windows.Shapes;
using System.Collections;
using PurchaseRequisitionSystem.Background;
using Infrastructure.SpecialObjects;
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.Models;
using System.Linq;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for SelectCostObjective.xaml
	/// </summary>
	public partial class SelectCostObjective : Window
	{
        private List<object> categories;
        private IEnumerable CostObjectives;
        private PrsEntities db;
        private AsyncWork async, async2,async3;
        private CostObjectiveModel SelectedData;
        private int categoryId;
        string _txtSearch;

        private AddItem addItemWindow;
        private EditItem editItemWindow;
        private Items selectItemWindow;

        private MainWindow mainWindow;
        private UserModel CurrentUser;
        private CostObjectiveScenarios Scenario;

		public SelectCostObjective()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            CurrentUser = App.Current.Properties["User"] as UserModel;
		}

        public SelectCostObjective(MainWindow mainWindow, AddItem itemWindow) : this() {
            this.addItemWindow = itemWindow;
            this.mainWindow = mainWindow;
            Scenario = CostObjectiveScenarios.AddItem;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow); async3 = new AsyncWork(mainWindow);
        }

        public SelectCostObjective(MainWindow mainWindow, EditItem itemWindow) : this() {
            this.editItemWindow = itemWindow;
            this.mainWindow = mainWindow;
            Scenario = CostObjectiveScenarios.EditItem;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow); async3 = new AsyncWork(mainWindow);
        }

        public SelectCostObjective(MainWindow mainWindow, Items itemWindow) : this() {
            this.selectItemWindow = itemWindow;
            this.mainWindow = mainWindow;
            Scenario = CostObjectiveScenarios.SelectItem;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow); async3 = new AsyncWork(mainWindow);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            async.Do(PopulateData, PopulateDataComplete);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            _txtSearch = txtSearch.Text;
            async2.Do(SearchData, OnSearchComplete);
        }

        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                var category = (KeyAndValue)e.AddedItems[0];
                categoryId = (int)category.Key;
                async3.Do(PopulateDataFromCategory, PopulateDataFromCategoryComplete);
            }
        }

        private void lstData_SelectionChanged_1(object sender, SelectionChangedEventArgs e) {
            SelectedData = (CostObjectiveModel)lstData.SelectedItem;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e) {
            if (lstData.SelectedIndex == -1) {
                MessageBox.Show("Please select a cost objective.");
                return;
            }

            if (Scenario == CostObjectiveScenarios.AddItem) {
                addItemWindow.SelectedCostObjective = SelectedData.Id;
                addItemWindow.tbCostObjectiveName.Text = SelectedData.Name;
            }
            else if (Scenario == CostObjectiveScenarios.EditItem) {
                editItemWindow.SelectedCostObjective = SelectedData.Id;
                editItemWindow.tbCostObjectiveName.Text = SelectedData.Name;
            }
            else if (Scenario == CostObjectiveScenarios.SelectItem) {
                selectItemWindow.CostObjectiveId = SelectedData.Id;
                selectItemWindow.tbCostObjectiveName.Text = SelectedData.Name;
            }
            this.Close();
        }

        private void PopulateData() {
            categories = new List<object>();

            if(categoryId != 0)
                CostObjectives = db.CostObjectives.Where(x => x.CategoryId == categoryId && x.CompanyId == CurrentUser.CompanyId).Select(x => new CostObjectiveModel { Id = x.Id, Name = x.Name, Company = x.Company.Name, Category = x.CostObjectiveCategory.Name });
            else
                CostObjectives = db.CostObjectives.Where(x=>x.CompanyId == CurrentUser.CompanyId).Select(x => new CostObjectiveModel { Id = x.Id, Name = x.Name, Company = x.Company.Name, Category = x.CostObjectiveCategory.Name });

            foreach (var type in db.CostObjectiveCategories) {
                KeyAndValue tmpType = new KeyAndValue(type.Id, type.Name);
                categories.Add(tmpType);
            }
        }

        private void PopulateDataComplete() {
            lstData.ItemsSource = CostObjectives;
            lstData.ClearFirstColumn();
            cbCategory.ItemsSource = categories;
            cbCategory.IsEnabled = true;
            mainWindow.stryLoading.Stop();
        }

        private void PopulateDataFromCategory() {
            if (categoryId != 0)
                CostObjectives = db.CostObjectives.Where(x => x.CategoryId == categoryId && x.CompanyId == CurrentUser.CompanyId).Select(x => new CostObjectiveModel { Id = x.Id, Name = x.Name, Company = x.Company.Name, Category = x.CostObjectiveCategory.Name });
            else
                CostObjectives = db.CostObjectives.Where(x => x.CompanyId == CurrentUser.CompanyId).Select(x => new CostObjectiveModel { Id = x.Id, Name = x.Name, Company = x.Company.Name, Category = x.CostObjectiveCategory.Name });
        }

        private void PopulateDataFromCategoryComplete() {
            lstData.ItemsSource = CostObjectives;
            lstData.ClearFirstColumn();
            mainWindow.stryLoading.Stop();
        }

        IEnumerable searchSource;
        private void SearchData() {
            if (categoryId != 0)
                searchSource = db.CostObjectives.Where(x => x.CategoryId == categoryId).Select(x => new CostObjectiveModel { Id = x.Id, Name = x.Name, Company = x.Company.Name, Category = x.CostObjectiveCategory.Name }).Where(x => x.Name.Contains(_txtSearch));
            else
                searchSource = db.CostObjectives.Where(x => x.CompanyId == CurrentUser.CompanyId).Select(x => new CostObjectiveModel { Id = x.Id, Name = x.Name, Company = x.Company.Name, Category = x.CostObjectiveCategory.Name }).Where(x => x.Name.Contains(_txtSearch));
        }

        private void OnSearchComplete() {
            lstData.ItemsSource = searchSource;
            lstData.ClearFirstColumn();
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e) {
            categoryId = 0;
            Window_Loaded(this, e);
        }
    }
}