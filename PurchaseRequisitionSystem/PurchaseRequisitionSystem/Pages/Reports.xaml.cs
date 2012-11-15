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
using Infrastructure.Helpers.Extensions;
using PurchaseRequisitionSystem.PDF;
using PurchaseRequisitionSystem.Background;
using PurchaseRequisitionSystem.Models;
using System.Data;
using System.Data.OleDb;
using Microsoft.Win32;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace PurchaseRequisitionSystem
{
	/// <summary>
	/// Interaction logic for Reports.xaml
	/// </summary>
	public partial class Reports : UserControl
	{
        private MainWindow mainWindow;
        private PrsEntities db;
        private PdfManager pdf;
        private AsyncWork async;
        private DateTime queryDateFrom;
        private DateTime queryDateTo;
        private IEnumerable<PrReportModel> Report;
        private IEnumerable<PrCompletedReportModels> ReportComplete;
        private int FilterCompanyId;
        

		public Reports()
		{
			this.InitializeComponent();
            db = new PrsEntities();
            pdf = new PdfManager();
		}

        public Reports(MainWindow mainWindow) : this() {
            // TODO: Complete member initialization
            this.mainWindow = mainWindow;
            async = new AsyncWork(mainWindow);
        }

        private void btnProceed_Click(object sender, RoutedEventArgs e) {
            //Selection for Comany
            if (comboCompany.SelectedIndex == -1) {
                MessageBox.Show("Please select a company first.");
                return;
            }
            var selectedCompany = (ComboBoxItem)comboCompany.SelectedItem;
            FilterCompanyId = selectedCompany.Uid.ToInt();

            // Date initialization
            queryDateFrom = dateFrom.SelectedDate.HasValue ? dateFrom.SelectedDate.Value : DateTime.Now.Date;
            queryDateTo = dateTo.SelectedDate.HasValue ? dateTo.SelectedDate.Value : queryDateFrom.AddMonths(1).Date;
            //If From overlaps To
            if (queryDateFrom > queryDateTo) {
                MessageBox.Show("Invalid Dates.\nPlease input again.");
                return;
            }

            // Query For Report
            async.Do(PopulateReport, PopulateReport_Complete);
        }

        private void PopulateReport() {
            Report = from r in db.PurchaseRequests
                     join u in db.Users on r.RequestedBy equals u.Id
                     join ir in db.mItemsRequests on r.Id equals ir.RequestId
                     join i in db.Items on ir.ItemId equals i.Id into final
                     from f in final.DefaultIfEmpty()
                     where u.CompanyId == FilterCompanyId && r.created_at >= queryDateFrom && r.created_at <= queryDateTo
                     select new PrReportModel()
                     {
                         ItemDescription = f.Description,
                         DepartmentName = u.Department.Name,
                         PrDateCreated = r.created_at.Value,
                         PrDateRequired = r.DateRequired,
                         PrNumber = r.PRNo,
                         PrStatus = r.Status
                     };

            ReportComplete = from r in db.PurchaseRequests
                     join u in db.Users on r.RequestedBy equals u.Id
                     join ir in db.mItemsRequests on r.Id equals ir.RequestId into final
                     from f in final.DefaultIfEmpty()
                     where (u.CompanyId == FilterCompanyId && r.created_at >= queryDateFrom && r.created_at <= queryDateTo) && r.IsFinished == true
                     select new PrCompletedReportModels()
                     {
                         ItemDescription = f.Item.Description,
                         UOM = f.Item.UnitOfMeasurement.Name,
                         CostObjective = f.CostObjective.Name,
                         Quantity = f.Quantity,
                         UnitPrice = f.Price.HasValue ? f.Price.Value : 0,
                         TotalAmount = f.Price.HasValue ? f.Price.Value * f.Quantity : 0,
                         Purpose = r.Purpose,
                         DepartmentName = u.Department.Name,
                         PrDateCreated = r.created_at.Value,
                         PrDateRequired = r.DateRequired,
                         PrNumber = r.PRNo,
                         PrStatus = r.Status
                     };
        }

        private void PopulateReport_Complete() {
            dgReport.ItemsSource = Report;
            CreateExcelReport(Report, ReportComplete);
        }

        private void CreateExcelReport(IEnumerable<PrReportModel> list, IEnumerable<PrCompletedReportModels> completedList) {
            SaveFileDialog fd = new SaveFileDialog();
            fd.DefaultExt = ".xlsx";
            fd.Title = "Do you want to save this document?";
            fd.ShowDialog();

            if (fd.FileName.IsNullOrEmpty()) {
                return;
            }

            FileInfo newFile = new FileInfo(fd.FileName);
            if (newFile.Exists) {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(fd.FileName);
            }

            using (ExcelPackage package = new ExcelPackage(newFile)) {
                // add a new worksheet to the empty workbook
                PrintAllRequests(list, package);
                PrintAllCompleteRequests(completedList, package);
                package.Save();

            }
        }

        private void PrintAllRequests(IEnumerable<PrReportModel> list, ExcelPackage package) {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report for All");
            worksheet.Cells["A1:D1"].Merge = true;
            worksheet.Cells["A1:D1"].Value = "PURCHASE REQUISITION REPORT";
            worksheet.Cells["A1:D1"].Style.Font.Bold = true;
            worksheet.Cells["A1:D1"].Style.Font.Size = 15;

            worksheet.Cells["A2:E2"].Value = queryDateFrom.ToLongDateString() + " - " + queryDateTo.ToLongDateString();
            worksheet.Cells["A2:E2"].Style.Font.Size = 15;
            worksheet.Cells["A2:E2"].Merge = true;

            //Add the headers
            worksheet.Cells[4, 1].Value = "Department";
            worksheet.Cells[4, 2].Value = "Item Description";
            worksheet.Cells[4, 3].Value = "PR Number";
            worksheet.Cells[4, 4].Value = "PR Date";
            worksheet.Cells[4, 5].Value = "PR Date Required";
            worksheet.Cells[4, 6].Value = "PR Status";

            //Add some items...
            int RowNumber = 5;
            foreach (var row in list) {
                worksheet.Cells[RowNumber, 1].Value = row.DepartmentName;
                worksheet.Cells[RowNumber, 2].Value = row.ItemDescription;
                worksheet.Cells[RowNumber, 3].Value = row.PrNumber;
                worksheet.Cells[RowNumber, 4].Value = row.PrDateCreated;
                worksheet.Cells[RowNumber, 5].Value = row.PrDateRequired;
                worksheet.Cells[RowNumber, 6].Value = row.PrStatus;
                RowNumber++;
            }

            worksheet.Column(4).Style.Numberformat.Format = @"mm-dd-yy";
            worksheet.Column(5).Style.Numberformat.Format = @"mm-dd-yy";

            worksheet.Cells.AutoFitColumns(0);
            worksheet.Cells["A4:F4"].AutoFilter = true;
            worksheet.Cells["A4:F4"].Style.Font.Bold = true;
        }

        private void PrintAllCompleteRequests(IEnumerable<PrCompletedReportModels> list, ExcelPackage package) {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Report for Completed");
            worksheet.Cells["A1:D1"].Merge = true;
            worksheet.Cells["A1:D1"].Value = "PURCHASE REQUISITION REPORT";
            worksheet.Cells["A1:D1"].Style.Font.Bold = true;
            worksheet.Cells["A1:D1"].Style.Font.Size = 15;

            worksheet.Cells["A2:E2"].Value = queryDateFrom.ToLongDateString() + " - " + queryDateTo.ToLongDateString();
            worksheet.Cells["A2:E2"].Style.Font.Size = 15;
            worksheet.Cells["A2:E2"].Merge = true;

            //Add the headers
            worksheet.Cells[4, 1].Value = "Department";
            worksheet.Cells[4, 2].Value = "Cost Objective";
            worksheet.Cells[4, 3].Value = "Date";
            worksheet.Cells[4, 4].Value = "PR Number";
            worksheet.Cells[4, 5].Value = "Item Description";
            worksheet.Cells[4, 6].Value = "Unit Price";
            worksheet.Cells[4, 7].Value = "Quantity";
            worksheet.Cells[4, 8].Value = "UOM";
            worksheet.Cells[4, 9].Value = "Total Amount";
            worksheet.Cells[4, 10].Value = "Purpose";
            worksheet.Cells[4, 11].Value = "Status";

            //Add some items...
            int RowNumber = 5;
            foreach (var row in list) {
                worksheet.Cells[RowNumber, 1].Value = row.DepartmentName;
                worksheet.Cells[RowNumber, 2].Value = row.CostObjective;
                worksheet.Cells[RowNumber, 3].Value = row.PrDateCreated;
                worksheet.Cells[RowNumber, 4].Value = row.PrNumber;
                worksheet.Cells[RowNumber, 5].Value = row.ItemDescription;
                worksheet.Cells[RowNumber, 6].Value = row.UnitPrice;
                worksheet.Cells[RowNumber, 7].Value = row.Quantity;
                worksheet.Cells[RowNumber, 8].Value = row.UOM;
                worksheet.Cells[RowNumber, 9].Value = row.TotalAmount;
                worksheet.Cells[RowNumber, 10].Value = row.Purpose;
                worksheet.Cells[RowNumber, 11].Value = row.PrStatus;
                RowNumber++;
            }

            double TotalUnitPrice = list.Sum(x => x.UnitPrice);
            double TotalNetPrice = list.Sum(x => x.TotalAmount);

            worksheet.Cells[RowNumber, 6].Value = TotalUnitPrice;
            worksheet.Cells[RowNumber, 9].Value = TotalNetPrice;

            worksheet.Cells[RowNumber, 6].Style.Font.Size = 12;
            worksheet.Cells[RowNumber, 9].Style.Font.Size = 12;
            worksheet.Cells[RowNumber, 6].Style.Font.Bold = true;
            worksheet.Cells[RowNumber, 9].Style.Font.Bold = true;

            worksheet.Column(3).Style.Numberformat.Format = @"mm-dd-yy";

            worksheet.Cells.AutoFitColumns(0);
            worksheet.Cells["A4:K4"].AutoFilter = true;
            worksheet.Cells["A4:K4"].Style.Font.Bold = true;
        }
	}
}