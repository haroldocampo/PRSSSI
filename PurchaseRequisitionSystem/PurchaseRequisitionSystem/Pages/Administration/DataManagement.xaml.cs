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
using System.Data.Objects;
using System.Linq;
using System.Collections;
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.Background;

namespace PurchaseRequisitionSystem {
    /// <summary>
    /// Interaction logic for DataManagement.xaml
    /// </summary>
    public partial class DataManagement : UserControl {
        private MainWindow mainWindow;
        private Type type;
        private PrsEntities db;
        private string tableName;
        private AsyncWork async, async2, async3;
        private DataModel SelectedData;

        string _txtName;
        string _txtSearch;

        public DataManagement() {
            this.InitializeComponent();
            db = new PrsEntities();
        }

        public DataManagement(MainWindow mainWindow, Type type)
            : this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            this.type = type;
            async = new AsyncWork(mainWindow); async2 = new AsyncWork(mainWindow); async3 = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            IEnumerable dataSource;
            if (type == new UnitOfMeasurement().GetType()) {
                tableName = "UnitOfMeasurements";
                dataSource = db.UnitOfMeasurements.Select(x => new DataModel { Id = x.Id, Name = x.Name });
                lstData.ItemsSource = dataSource;
            } else if (type == new ItemBrand().GetType()) {
                tableName = "ItemBrands";
                dataSource = db.ItemBrands.Select(x => new DataModel { Id = x.Id, Name = x.Name });
                lstData.ItemsSource = dataSource;
            } 
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            _txtName = txtName.Text;
            if (_txtName.IsNullOrEmpty()) {
                txtName.FlagAsError();
                return;
            } txtName.FlagAsNormal();
            async.Do(AddData, OnAsyncAddUpdateComplete);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
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

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            _txtSearch = txtSearch.Text;
            async2.Do(SearchData, OnSearchComplete);
        }

        private void lstData_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SelectedData = (DataModel)lstData.SelectedItem;
            txtName.Text = SelectedData.Name;
        }


        private void UpdateData() {
            string command = "Update {0} SET Name='{1}' WHERE Id={2}".WithTokens(tableName, _txtName, SelectedData.Id);
            db.ExecuteStoreCommand(command, null);
        }

        private void AddData() {
            db.ExecuteStoreCommand("Insert into {0} (Name) values ('{1}')".WithTokens(tableName, _txtName), null);
        }

        private void OnAsyncAddUpdateComplete() {
            mainWindow.TFrame.ShowPage(new DataManagement(mainWindow, type));
        }

        IEnumerable searchSource;
        private void SearchData() {
            if (type == new UnitOfMeasurement().GetType()) {
                searchSource = db.UnitOfMeasurements.Select(x => new DataModel { Id = x.Id, Name = x.Name }).Where(x => x.Name.Contains(_txtSearch));
            }
            else if (type == new ItemBrand().GetType()) {
                searchSource = db.ItemBrands.Select(x => new DataModel { Id = x.Id, Name = x.Name }).Where(x => x.Name.Contains(_txtSearch));
            }
        }

        private void OnSearchComplete() {
            lstData.ItemsSource = searchSource;
        }
    }

    public class DataModel {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}