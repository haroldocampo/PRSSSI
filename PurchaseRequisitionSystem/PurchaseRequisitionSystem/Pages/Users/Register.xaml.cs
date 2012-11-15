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
using Infrastructure.SpecialObjects;
using Infrastructure.Helpers.Extensions;
using Infrastructure.Security;
using System.Linq;
using System.Threading;
using PurchaseRequisitionSystem.Models;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for Register.xaml
	/// </summary>
	public partial class Register : UserControl
	{
        // Code Variables
        private MainWindow mainWindow;
        private PrsEntities db;
        private AsyncWork background;
        private AsyncWork background2;
        private AsyncWork background3;
        private AsyncWork asyncUserCheck;
        private int companyId;
        private bool UserExisting = true;
        private List<object> companies;
        private List<object> branches;
        private List<object> departments;
        private List<object> userTypes;

        //UI Variables
        string Username;
        string Password;
        string LastName;
        string FirstName;
        string ContactNo;
        string Email;
        int Company;
        int Branch;
        int Department;
        int UserType;
        string Street;
        string City;
        string Zip;

        private UserModel CurrentUser;

		public Register()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public Register(MainWindow mainWindow) : this(){
            this.mainWindow = mainWindow;
            background = new AsyncWork(mainWindow);
            background2 = new AsyncWork(mainWindow);
            background3 = new AsyncWork(mainWindow);
            asyncUserCheck = new AsyncWork(mainWindow);
            CurrentUser = App.Current.Properties["User"] as UserModel;
            if (CurrentUser == null) {
                stackUserType.Visibility = Visibility.Collapsed;
            }
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e) {
            background.Do(PopulateCompanies, PopulateCompaniesComplete);
        }

        private void comboCompany_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //REFACTOR THIS
            if (e.AddedItems.Count > 0) {
                var company = (KeyAndValue)e.AddedItems[0];
                companyId = (int)company.Key;
                background2.Do(PopulateDepAndBranches, PopulateDepAndBranchesComplete);
            }
        }

        private void PopulateCompanies(){
            companies = new List<object>();
            userTypes = new List<object>();
            foreach (var company in db.Companies) {
                KeyAndValue tmpCompany = new KeyAndValue(company.Id, company.Name);
                companies.Add(tmpCompany);
            }

            foreach (var type in db.UserTypes) {
                KeyAndValue tmpType = new KeyAndValue(type.Id, type.UserType1);
                userTypes.Add(tmpType);
            }
        }

        private void PopulateCompaniesComplete(){
            comboUserType.ItemsSource = userTypes;
            comboCompany.ItemsSource = companies;
            comboCompany.IsEnabled = true;
            comboUserType.IsEnabled = true;
            mainWindow.stryLoading.Stop();
        }

        private void PopulateDepAndBranches() {
            departments = new List<object>();
            foreach (var department in db.Departments.Where(x => x.CompanyId == companyId)) {
                KeyAndValue tmpdepartment = new KeyAndValue(department.Id, department.Name);
                departments.Add(tmpdepartment);
            }

            branches = new List<object>();
            foreach (var branch in db.Branches.Where(x => x.CompanyId == companyId)) {
                KeyAndValue tmpdepartment = new KeyAndValue(branch.Id, branch.Name);
                branches.Add(tmpdepartment);
            }
        }

        private void PopulateDepAndBranchesComplete() {
            //Department
            comboDepartment.ItemsSource = departments;
            comboDepartment.IsEnabled = true;

            comboBranch.ItemsSource = branches;
            comboBranch.IsEnabled = true;

            mainWindow.stryLoading.Stop();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e) {
            bool Validated = false;
            var Company = (KeyAndValue)comboCompany.SelectedItem;
            var Branch = (KeyAndValue)comboBranch.SelectedItem;
            var Department = (KeyAndValue)comboDepartment.SelectedItem;
            var UserType = (KeyAndValue)comboUserType.SelectedItem;
            
            ValidatePasswords(txtPassword, txtConfirmPassword, out Validated);
            ValidateRequiredFields(txtUsername, txtPassword, txtFirstName, txtLastName, out Validated);
            ValidateComboBoxes(Company, Branch, Department, UserType, out Validated);

            if (!Validated) {
                MessageBox.Show("Please Correct the Highlighted Fields");
                return;
            }

            //Insert Validated Form Values
            PrepareValuesForInsert(Company, Branch, Department, UserType);

            CheckExistingUser();

            if (UserExisting) {
                MessageBox.Show("Username already exists");
                return;
            }

            // Save User
            background3.Do(SaveUserToDB);

            //Redirect to Login Page
            if (CurrentUser == null) {
                mainWindow.TFrame.ShowPage(new Login(mainWindow));
            }
            else {
                mainWindow.TFrame.ShowPage(new Purchasing(mainWindow));
            }
        }

        private void SaveUserToDB() {
            var NewUser = db.Users.CreateObject();
            NewUser.Id = Guid.NewGuid();
            NewUser.Username = Username;
            NewUser.Password = Salt.ComputeHash(Password,"SHA512", null);
            NewUser.LastName = LastName;
            NewUser.FirstName = FirstName;
            NewUser.ContactNumber = ContactNo;
            NewUser.Email = Email;
            NewUser.CompanyId = this.Company;
            NewUser.BranchId = this.Branch;
            NewUser.DepartmentId = this.Department;
            NewUser.UserTypeId = this.UserType;
            NewUser.AddressCity = City;
            NewUser.AddressStreet = Street;
            NewUser.AddressZip = Zip;
            db.AddToUsers(NewUser);
            db.SaveChanges();
        }

        private void CheckExistingUser() {
            UserExisting = db.Users.Where(x => x.Username == Username).Any();
        }

        private void PrepareValuesForInsert(KeyAndValue Company, KeyAndValue Branch, KeyAndValue Department, KeyAndValue UserType) {
            Username = txtUsername.Text;
            Password = txtPassword.Password;
            LastName = txtLastName.Text;
            FirstName = txtFirstName.Text;
            ContactNo = txtContactNo.Text;
            Email = txtEmail.Text;
            this.Company = (int)Company.Key;
            this.Branch = (int)Branch.Key;
            this.Department = (int)Department.Key;
            if (CurrentUser != null) {
                this.UserType = (int)UserType.Key;
            }
            else {
                this.UserType = (int)Enums.UserTypes.Requisitioner;
            }
            Street = txtStreet.Text;
            City = txtCity.Text;
            Zip = txtZip.Text;
        }

        private void ValidateRequiredFields(TextBox txtUsername, PasswordBox txtPassword, TextBox txtFirstName, TextBox txtLastName, out bool Validated) {
            Validated = false;
            if(txtUsername.Text.IsNullOrEmpty()){
                txtUsername.FlagAsError();
                return;
            } txtUsername.FlagAsCorrect();

            if (txtPassword.Password.IsNullOrEmpty()) {
                txtPassword.FlagAsError();
                return;
            } txtPassword.FlagAsCorrect();

            if (txtFirstName.Text.IsNullOrEmpty()) {
                txtFirstName.FlagAsError();
                return;
            } txtFirstName.FlagAsCorrect();

            if (txtLastName.Text.IsNullOrEmpty()) {
                txtLastName.FlagAsError();
                return;
            } txtLastName.FlagAsCorrect();
            Validated = true;
        }

        private void ValidatePasswords(PasswordBox txtPassword, PasswordBox txtConfirmPassword, out bool Validated) {
            Validated = false;
            if (txtPassword.Password != txtConfirmPassword.Password) {
                txtPassword.FlagAsError();
                txtConfirmPassword.FlagAsError();
                return;
            }
            Validated = true;
        }

        private void ValidateComboBoxes(KeyAndValue Company, KeyAndValue Branch, KeyAndValue Department, KeyAndValue UserType, out bool Validated) {
            Validated = false;

            if (UserType == null && CurrentUser != null) {
                comboUserType.FlagAsError();
                return;
            } comboUserType.FlagAsCorrect();

            if(Company == null) {
                comboCompany.FlagAsError();
                return;
            } comboCompany.FlagAsCorrect();

            if (Branch == null) {
                comboBranch.FlagAsError();
                return;
            } comboBranch.FlagAsCorrect();

            if (Department == null) {
                comboDepartment.FlagAsError();
                return;
            } comboDepartment.FlagAsCorrect();

            Validated = true;
        }
	}
}