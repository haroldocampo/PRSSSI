using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Data;

namespace PurchaseRequisitionSystem.Background {
    public class AsyncWork{
        private BackgroundWorker background;
        private MainWindow mainWindow;
        bool ExceptionOccured = false;
        public AsyncWork(MainWindow mainWindow) {
            this.mainWindow = mainWindow;
            background = new BackgroundWorker();
        }

        public void Do(Action work) {
            mainWindow.stryLoading.Begin();
            mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(239, 147, 12));
            background.DoWork += (q, w) =>
            {
                try {
                    work();
                }
                catch (EntityException) {
                    ExceptionOccured = true;
                    MessageBox.Show("Could not connect to the server. \n\nPlease check your connection.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                } 
            };
            background.RunWorkerCompleted += (q, w) =>
            {
                mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(7, 35, 92));
                mainWindow.stryLoading.Stop();
                if(!ExceptionOccured)
                    MessageBox.Show("The operation has been completed", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                else {
                    mainWindow.TFrame.ShowPage(new Login(mainWindow));
                }
                background.Dispose();
            };
            if (!background.IsBusy) {
                background.RunWorkerAsync();
            }
            else {
                mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(241, 5, 0));
                return;
            }
        }

        public void Do<T>(Action<T> work, T parameter, Action work_completed) {
            mainWindow.stryLoading.Begin();
            mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(239, 147, 12));
            background.DoWork += (q, w) =>
            {
                try {
                    work(parameter);
                }
                catch (EntityException) {
                    ExceptionOccured = true;
                    MessageBox.Show("Could not connect to the server. \n\nPlease check your connection.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                } 
            };
            background.RunWorkerCompleted += (q, w) =>
            {
                mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(7, 35, 92));
                mainWindow.stryLoading.Stop();
                if(!ExceptionOccured)
                    work_completed();
                else {
                    mainWindow.TFrame.ShowPage(new Login(mainWindow));
                }
                background.Dispose();
            };
            if (!background.IsBusy) {
                background.RunWorkerAsync();
            }
            else {
                mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(241, 5, 0));
                return;
            }
        }

        public void Do(Action work, Action work_completed) {
            mainWindow.stryLoading.Begin();
            mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(239, 147, 12));
            background.DoWork += (q, w) =>
            {
                try {
                    work();
                } catch (EntityException) {
                    ExceptionOccured = true;
                    MessageBox.Show("Could not connect to the server. \n\nPlease check your connection.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                } 
            };
            background.RunWorkerCompleted += (q, w) =>
            {
                mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(7, 35, 92));
                mainWindow.stryLoading.Stop();
                if (!ExceptionOccured) {
                    work_completed();
                }
                else {
                    mainWindow.TFrame.ShowPage(new Login(mainWindow));
                }
                background.Dispose();
            };
            if (!background.IsBusy) {
                background.RunWorkerAsync();
            }
            else {
                mainWindow.grdLoader.Background = new SolidColorBrush(Color.FromRgb(241, 5, 0));
                return;
            }
        }
    }
}
