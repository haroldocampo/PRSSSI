using System;
using System.Collections.Generic;
using System.Linq;
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
using WpfPageTransitions;
using System.Windows.Media.Animation;
using PurchaseRequisitionSystem.Models;
using PurchaseRequisitionSystem.Enums;
using PurchaseRequisitionSystem.PDF;
using System.IO;
using Microsoft.Win32;

namespace PurchaseRequisitionSystem {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        //Public Fields
        public Storyboard stryLoading;

        //Main Window Constructor
        public MainWindow() {
            InitializeComponent();
            stryLoading = (Storyboard)FindResource("stryLoading");
            this.Dispatcher.UnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);
        }

        void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            MessageBox.Show(e.Exception.Message, "An Error Has Occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            if (App.Current.Properties["User"] == null) {
                Login login = new Login(this);
                TFrame.ShowPage(login);
            }
            else {
                var User = (UserModel)App.Current.Properties["User"];
                switch (User.UserTypeId) {
                    case (int)UserTypes.Requisitioner:
                        TFrame.ShowPage(new Requisitioner(this));
                        break;
                    case (int)UserTypes.PlantManager:
                        TFrame.ShowPage(new OverallHead(this));
                        break;
                    case (int)UserTypes.OverallHead:
                        TFrame.ShowPage(new OverallHead(this));
                        break;
                    case (int)UserTypes.Purchasing:
                        TFrame.ShowPage(new Purchasing(this));
                        break;
                    case (int)UserTypes.VicePresident:
                        TFrame.ShowPage(new VicePresident(this));
                        break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            WelcomePage welcome = new WelcomePage(this);
            TFrame.ShowPage(welcome);
        }

        private void btnPurchaseRequest_Click(object sender, RoutedEventArgs e) {
            TFrame.ShowPage(new PurchaseRequestScreen(this));
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e) {
        }

        private void btnMyRequests_Click(object sender, RoutedEventArgs e) {
            MyRequests myRequestDialog = new MyRequests(this);
            TFrame.ShowPage(myRequestDialog);
        }
    }
}
