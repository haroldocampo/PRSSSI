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
using Infrastructure.Security;
using PurchaseRequisitionSystem.Background;
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : UserControl
	{
        public UserModel User { get; set; }

        private MainWindow mainWindow;
        private AsyncWork async;
        private PrsEntities db;

        string Username;
        string Password;

        bool UserExists;
        bool UserVerified;

		public Login()
		{
			this.InitializeComponent();
            db = new PrsEntities();
		}

        public Login(MainWindow mainWindow) : this() {
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow);
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e) {
            mainWindow.TFrame.ShowPage(new Register(mainWindow));
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            Username = txtUsername.Text;
            Password = txtPassword.Password;

            async.Do(VerifyUser, VerifyUser_Complete);
        }

        private void VerifyUser(){
            var user = db.Users.Where(x => x.Username == Username).Select(x => new UserModel { FirstName = x.FirstName, LastName = x.LastName, Id = x.Id, UserTypeId = x.UserType.Id, AccessLevel = x.UserType.AccessLevel, Password = x.Password, CompanyName = x.Company.Name, DepartmentName = x.Department.Name, CompanyId = x.CompanyId, Email = x.Email });
            UserExists = user.Any();

            if (UserExists) {
                User = user.SingleOrDefault();
                UserVerified = Salt.VerifyHash(Password, "SHA512", User.Password);
            }
        }

        private void VerifyUser_Complete() {
            if (!UserVerified || !UserExists) {
                MessageBox.Show("Username or Password is Incorrect.");
                mainWindow.TFrame.ShowPage(new Login(mainWindow));
                return;
            }

            App.Current.Properties["User"] = User;

            ImageBrush image = new ImageBrush();
            image.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Purchase Requisition System;component/Content/Home.png"));
            mainWindow.btnHomeLogin.Background = image;
            mainWindow.btnHomeLogin.Content = "Home";

            switch (User.UserTypeId) {
                case (int)UserTypes.Requisitioner:
                    mainWindow.TFrame.ShowPage(new Requisitioner(mainWindow));
                    break;
                case (int)UserTypes.PlantManager:
                    mainWindow.TFrame.ShowPage(new OverallHead(mainWindow));
                    break;
                case (int)UserTypes.OverallHead:
                    mainWindow.TFrame.ShowPage(new OverallHead(mainWindow));
                    break;
                case (int)UserTypes.Purchasing:
                    mainWindow.TFrame.ShowPage(new Purchasing(mainWindow));
                    break;
                case (int)UserTypes.VicePresident:
                    mainWindow.TFrame.ShowPage(new VicePresident(mainWindow));
                    break;
            }

            mainWindow.btnMyRequests.IsEnabled = true;
            mainWindow.btnPurchaseRequest.IsEnabled = true;
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                btnLogin_Click(this, e);
            }
        }

        private void txtUsername_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                btnLogin_Click(this, e);
            }
        }
	}
}