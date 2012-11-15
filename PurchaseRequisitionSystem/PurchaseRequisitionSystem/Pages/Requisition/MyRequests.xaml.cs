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
using PurchaseRequisitionSystem.Background;
using System.Collections;
using PurchaseRequisitionSystem.Models;
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for MyRequests.xaml
	/// </summary>
	public partial class MyRequests : UserControl
	{
        private PrsEntities db;
        private Guid UserId;
        private MainWindow mainWindow;
        private AsyncWork asyncSearch;
        private UserModel User;

		public MyRequests()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            User = (UserModel)App.Current.Properties["User"];
            UserId = User.Id;
		}

        public MyRequests(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
            asyncSearch = new AsyncWork(mainWindow);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            allRequests = db.PurchaseRequests.Where(x => x.User.Id == User.Id).Select(x =>
                new RequestModel
                {
                    Id = x.Id,
                    PRNumber = x.PRNo,
                    PRNumberModel = x.PRNoModel,
                    Purpose = x.Purpose,
                    PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                    RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                    DateRequired = x.DateRequired,
                    DateRequested = x.created_at.Value,
                    Status = x.Status,
                    IsEditable = x.IsEditable
                });
            dgRequests.ItemsSource = allRequests;
        }

        private int searchText;
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            searchText = txtSearch.Text.ToInt();
            asyncSearch.Do(SearchRequest, SearchComplete);
        }
        private IEnumerable MyRequestsList;
        private IQueryable<RequestModel> allRequests;
        private void SearchRequest() {
            MyRequestsList = db.PurchaseRequests.Where(x => x.User.Id == User.Id && x.PRNo == searchText).Select(x =>
                new RequestModel
                {
                    Id = x.Id,
                    PRNumber = x.PRNo,
                    PRNumberModel = x.PRNoModel,
                    Purpose = x.Purpose,
                    PRType = x.PRType_IsStock == true ? "Stock" : "Non-Stock",
                    RequestedBy = x.User.LastName + ", " + x.User.FirstName,
                    DateRequired = x.DateRequired,
                    DateRequested = x.created_at.Value,
                    Status = x.Status,
                    IsEditable = x.IsEditable
                });
        }

        private void SearchComplete() {
            mainWindow.stryLoading.Stop();
            dgRequests.ItemsSource = MyRequestsList;
        }

        private void btnView_Click(object sender, RoutedEventArgs e) {
            if (dgRequests.SelectedIndex == -1) {
                MessageBox.Show("Please select a request from the list");
                return;
            }

            var model = (RequestModel)dgRequests.SelectedItem;

            ViewRequest viewDialog = new ViewRequest(model, mainWindow);

            viewDialog.ShowDialog();
        }

        private void dgRequests_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            //if (dgRequests.SelectedIndex != -1) {
            //    btnView_Click(this, e);
            //}
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            Button selectedButton = sender as Button;
            var model = (RequestModel)selectedButton.DataContext;

            EditRequisition editPage = new EditRequisition(mainWindow, model.Id);
            mainWindow.TFrame.ShowPage(editPage);
        }
	}
}