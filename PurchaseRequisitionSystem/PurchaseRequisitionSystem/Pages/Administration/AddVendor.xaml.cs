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
using Infrastructure.Helpers.Extensions;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for VendorManagement.xaml
	/// </summary>
	public partial class AddVendor : UserControl
	{
        private AsyncWork async;
        private PrsEntities db;
        private MainWindow mainWindow;

        //UI Variables
        string VendorName;
        string TIN;
        string Terms;
        string Email;
        string Telephone;
        string ContactPerson;
        string Address;
        string FaxNumber;

		public AddVendor()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public AddVendor(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow);
        }

        private void btnAddVendor_Click(object sender, RoutedEventArgs e) {
            bool IsValid = false;
            PrepareForInsert();
            ValidateForm(out IsValid);

            if(!IsValid){
                MessageBox.Show("Please try to correct \nthe highlighted fields.","Oops!",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            async.Do(SaveVendor, SaveVendor_Complete);
        }

        private void SaveVendor() {
            var newVendor = db.Vendors.CreateObject();
            newVendor.Name = VendorName;
            newVendor.TIN = TIN;
            newVendor.Terms = Terms;
            newVendor.Email = Email;
            newVendor.Telephone = Telephone;
            newVendor.FaxNumber = FaxNumber;
            newVendor.ContactPerson = ContactPerson;
            newVendor.Address = Address;
            db.Vendors.AddObject(newVendor);
            db.SaveChanges();
        }

        private void SaveVendor_Complete() {
            MessageBox.Show("Vendor has been added to the system");
            mainWindow.TFrame.ShowPage(new Purchasing(mainWindow));
        }

        private void PrepareForInsert(){
            VendorName = txtVendorName.Text;
            TIN = txtTin.Text;
            Terms = txtTerms.Text;
            Email = txtEmail.Text;
            Address = txtAddress.Text;
            Telephone = txtTelephone.Text;
            ContactPerson = txtContactPerson.Text;
            
            FaxNumber = txtFaxNumber.Text;
        }

        private void ValidateForm(out bool IsValid) {
            IsValid = false;

            if (VendorName.IsNullOrEmpty()) {
                txtVendorName.FlagAsError();
                return;
            } txtVendorName.FlagAsNormal();

            if (TIN.IsNullOrEmpty()) {
                txtTin.FlagAsError();
                return;
            } txtTin.FlagAsNormal();

            if (Terms.IsNullOrEmpty() || !Terms.IsInteger()) {
                txtTerms.FlagAsError();
                return;
            } txtTerms.FlagAsNormal();

            if (Email.IsNullOrEmpty() || !Email.IsEmail()) {
                txtEmail.FlagAsError();
                return;
            } txtEmail.FlagAsNormal();

            IsValid = true;
        }
	}
}