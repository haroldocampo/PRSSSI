using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using PurchaseRequisitionSystem.Models;
using Infrastructure.Helpers.Extensions;
using System.Windows.Media.Imaging;
using PurchaseRequisitionSystem.Enums;

namespace PurchaseRequisitionSystem.PDF {
    public class PdfManager {

        private static readonly BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        private const string sonicLogo = "/Content/company_logos/sonic.gif";
        private const string somicoLogo = "/Content/company_logos/somico.gif";
        private const string steeltechLogo = "/Content/company_logos/steeltech.gif";
        private PdfContentByte page;
        
        private Document CurrentDoc;

        public void RenderRfq(Stream stream, VendorModel vendor, IEnumerable<ItemModel> items, UserModel user, int RfqNumber, PurchaseRequest Pr, DateTime DateRequested) {
            Document document = new Document(PageSize.LETTER);

            try {
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Create a page in the document and add it to the bottom layer
                document.NewPage();
                //Pass Document to this
                CurrentDoc = document;

                page = writer.DirectContentUnder;
                page.BeginText();
                int top = (int)document.GetTop(0);
                SetFontSizeTo(16);
                PrintText("REQUEST FOR QUOTATION (THIS IS NOT AN ORDER)", 20, 10);
                SetFontSizeTo(10);

                //Header
                PrintText("Company Name: {0}".WithTokens(Pr.User.Company.Name), 20, 30); // Company Name
                PrintText("Address: 8th Floor Gedisco Tower, 534 Asuncion St., Binondo, Manila", 20, 45); // Company Address
                PrintText("Purchasing Department", 20, 60);
                PrintText("Issued By: {0} {1}".WithTokens(user.FirstName, user.LastName), 20, 75); // Requested By
                PrintTextRight("RFQ NO: {0}".WithTokens(RfqNumber), 70, 30); // RFQ Number
                PrintTextRight("Date: {0}".WithTokens(DateTime.Now.ToShortDateString()), 70, 45); // Date Created
                PrintTextRight("PR No: {0}".WithTokens(Pr.PRNo), 70, 60); // PR Number

                //Body
                SetFontSizeTo(14);
                PrintText("BID/OFFER/QUOTE MUST BE RECEIVED BY:", 20, 105); // Requested By
                PrintTextRight("Date: {0}".WithTokens(DateRequested.ToShortDateString()), 95, 105); // Date Required
                SetFontSizeTo(10);
                PrintText("Attention: {0} {1}".WithTokens(user.FirstName, user.LastName), 40, 125); // Vendor Contact Person
                PrintText("Tel No: {0}".WithTokens("+63 2 244 9296"), 40, 140); // Vendor Tel No
                PrintText("Email Address: {0}".WithTokens(user.Email), 40, 155); // Vendor Email
                PrintText("Fax No: {0}".WithTokens("+63 2 241 7826"), 240, 125); // Vendor Fax No
                SetFontSizeTo(8);
                PrintText("Unless otherwise requested, quote on each item separately. Unit Prices shall be shown. If unable to furnish", 20, 175); // Vendor Email
                PrintText("items as specified, submit sample and/or descriptive specifications of substitute offered.", 20, 185); // Vendor Email

                //Items
                SetFontSizeTo(8);
                PrintText("_______________________________________________________________________________________________________________________", 20, 210);
                PrintText("Item No.", 20, 205);
                PrintText("Item Description", 70, 205);
                PrintText("Quantity", 370, 205);
                PrintText("Unit Price", 420, 205);
                PrintText("Discount", 470, 205);
                PrintText("Total", 520, 205);

                //// Items..
                //int staryY = 225;
                //int itemCount = 1;
                //foreach (var item in items) {
                //    PrintItem(itemCount.ToString(), item.Description, item.Quantity.ToString(), ref staryY);
                //    itemCount++;
                //}
                //PrintText("********** NOTHING FOLLOWS **********", 75, staryY + 15);

                // START HERE ----------------------------
                Paragraph Grid = new Paragraph("\n\n\n\n\n\n\n\n\n");
                Grid.Alignment = 1;
                Grid.SpacingAfter = 35f;
                document.Add(Grid);

                Paragraph Grid2 = new Paragraph();
                Grid2.Alignment = 1;
                Grid2.SpacingBefore = 35f;
                //Start Table -----------------------------------
                PdfPTable rightTable = new PdfPTable(7);
                int[] widths = { 1, 1, 1, 7, 2, 2, 2 };
                rightTable.SetWidths(widths);
                rightTable.TotalWidth = 570f;
                rightTable.LockedWidth = true;
                Font tableFont = new Font(Font.FontFamily.HELVETICA, 8);

                rightTable.HorizontalAlignment = Element.ALIGN_CENTER;

                rightTable.AddCell(new Phrase("Item No.", tableFont));
                rightTable.AddCell(new Phrase("Item Description", tableFont));
                rightTable.AddCell(new Phrase("Quantity", tableFont));
                rightTable.AddCell(new Phrase("Unit Price", tableFont));
                rightTable.AddCell(new Phrase("Discount", tableFont));
                rightTable.AddCell(new Phrase("Total Amount", tableFont));

                rightTable.HorizontalAlignment = Element.ALIGN_LEFT;

                int itemCount = 1;
                foreach (var item in items) {
                    rightTable.AddCell(new Phrase(item.ItemNumber.ToString(), tableFont));
                    rightTable.AddCell(new Phrase(item.Description, tableFont));
                    rightTable.AddCell(new Phrase(item.Quantity.ToString(), tableFont));
                    rightTable.AddCell(new Phrase("", tableFont));
                    rightTable.AddCell(new Phrase("", tableFont));
                    rightTable.AddCell(new Phrase("", tableFont));
                    itemCount++;
                }
                //Nothing Follows
                rightTable.AddCell(new Phrase("", tableFont)); rightTable.AddCell(new Phrase("", tableFont)); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase("******** NOTHING FOLLOWS ********", tableFont)); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase(""));

                Grid2.Add(rightTable);
                document.Add(Grid2);
                // End of table -----------------------------------

                // Footer
                SetFontSizeTo(10);
                PrintTextBottom("If accepted within _____ days, the undersigned offers and agrees to honor this quotation.", 20, 85);
                PrintTextBottom("Print Name of Person:", 20, 65);
                PrintTextBottom("Company Name:", 20, 50);
                PrintTextBottom("Address:", 20, 35);
                PrintTextBottom("Email Address:", 20, 20);

                PrintTextBottom("No. of Days to Deliver:", 200, 65);
                PrintTextBottom("Telephone No.:", 200, 50);
                PrintTextBottom("Fax No.:", 200, 35);

                page.EndText();

                writer.Flush();
            }
            finally {
                document.Close();
            }
        }

        public void RenderCanvasSheet(Stream stream, IEnumerable<CanvasItemModel> items, UserModel currentUser,UserModel requestedBy, PurchaseRequest Pr, string[] Vendors) {
            Document document = new Document(PageSize.LETTER);

            try {
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Create a page in the document and add it to the bottom layer
                document.NewPage();
                //Pass Document to this
                CurrentDoc = document;

                page = writer.DirectContentUnder;
                page.BeginText();
                int top = (int)document.GetTop(0);
                SetFontSizeTo(16);
                PrintText("CANVASS SHEET", 20, 10);
                SetFontSizeTo(10);

                //Header
                PrintText("Company Name: " + requestedBy.CompanyName, 20, 30); // Company Name
                PrintText("Department: " + requestedBy.DepartmentName, 20, 45); // Company Address
                PrintText("Purpose: " + Pr.Purpose, 20, 60); // Purpose
                PrintTextRight("Date: {0}".WithTokens(DateTime.Now.ToShortDateString()), 70, 30); // Date Created
                PrintTextRight("PR No: {0}".WithTokens(Pr.PRNo), 70, 45); // PR Number

                SetFontSizeTo(8);

                // START HERE ----------------------------
                Paragraph Grid = new Paragraph("\n");
                Grid.Alignment = 1;
                Grid.SpacingAfter = 10f;
                document.Add(Grid);

                Paragraph Grid2 = new Paragraph();
                Grid2.Alignment = 1;
                Grid2.SpacingBefore = 35f;
                //Start Table -----------------------------------
                PdfPTable rightTable = new PdfPTable(7);
                int[] widths = { 1, 7, 2, 2, 2, 2, 2 };
                rightTable.SetWidths(widths);
                rightTable.TotalWidth = 570f;
                rightTable.LockedWidth = true;
                Font tableFont = new Font(Font.FontFamily.HELVETICA, 8);

                PdfPCell rightAlignedCell = new PdfPCell();
                rightAlignedCell.HorizontalAlignment = Element.ALIGN_CENTER;

                //Headers
                rightAlignedCell.Phrase = new Phrase("Item No.", tableFont); rightTable.AddCell(rightAlignedCell); 
                rightAlignedCell.Phrase = new Phrase("Item Description", tableFont); rightTable.AddCell(rightAlignedCell);
                rightAlignedCell.Phrase = new Phrase("Quantity", tableFont); rightTable.AddCell(rightAlignedCell);
                rightAlignedCell.Phrase = new Phrase("UOM", tableFont); rightTable.AddCell(rightAlignedCell);
                rightAlignedCell.Phrase = new Phrase(Vendors[0], tableFont); rightTable.AddCell(rightAlignedCell);
                rightAlignedCell.Phrase = new Phrase(Vendors[1], tableFont); rightTable.AddCell(rightAlignedCell);
                rightAlignedCell.Phrase = new Phrase(Vendors[2], tableFont); rightTable.AddCell(rightAlignedCell);

                rightAlignedCell.HorizontalAlignment = Element.ALIGN_RIGHT;

                foreach (var item in items) {
                    string priceVendor1 = item.PriceVendor1 == 0 ? "NONE" : item.PriceVendor1.ToString();
                    string priceVendor2 = item.PriceVendor2 == 0 ? "NONE" : item.PriceVendor2.ToString();
                    string priceVendor3 = item.PriceVendor3 == 0 ? "NONE" : item.PriceVendor3.ToString();
                    rightTable.AddCell(new Phrase(item.ItemNumber.ToString(), tableFont));
                    rightTable.AddCell(new Phrase(item.ItemDescription, tableFont));
                    rightAlignedCell.Phrase = new Phrase(item.Quantity.ToString(), tableFont); rightTable.AddCell(rightAlignedCell); 
                    rightTable.AddCell(new Phrase(item.UOM, tableFont));
                    rightAlignedCell.Phrase = new Phrase(priceVendor1, tableFont); rightTable.AddCell(rightAlignedCell);
                    rightAlignedCell.Phrase = new Phrase(priceVendor2, tableFont); rightTable.AddCell(rightAlignedCell);
                    rightAlignedCell.Phrase = new Phrase(priceVendor3, tableFont); rightTable.AddCell(rightAlignedCell); 
                }
                Grid2.Add(rightTable);
                document.Add(Grid2);

                // Footer
                SetFontSizeTo(10);
                PrintTextBottom("Requested By:", 20, 105);
                PrintTextBottom("{0} {1}".WithTokens(requestedBy.FirstName, requestedBy.LastName), 20, 35);

                PrintTextBottom("Prepared By:", 265, 105);
                PrintTextBottom("{0} {1}".WithTokens(currentUser.FirstName, currentUser.LastName), 265, 35);

                PrintTextBottom("Approved By:", 450, 105);
                PrintTextBottom("{0}".WithTokens("______________________"), 450, 35);

                page.EndText();

                writer.Flush();
            }
            finally {
                document.Close();
            }
        }

        public void RenderPurchaseOrder(Stream stream, int PoNumber, IEnumerable<PoItemModel> PoItems, int RequestNumber, string VendorName, string VendorCode, string VendorAddress, DateTime DateCreated, DateTime DeliveryDate, UserModel RequestUser, UserModel CurrentUser) {
            Document document = new Document(PageSize.LETTER);

            try {
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Create a page in the document and add it to the bottom layer
                document.NewPage();
                //Pass Document to this
                CurrentDoc = document;

                string logoLocation = sonicLogo;
                float logoPosition = 50;

                string CurrentCompany = RequestUser.CompanyName;
                //Image
                if (RequestUser.CompanyId == (int)CompanyIds.SonicSteel) {
                    logoLocation = sonicLogo;
                    logoPosition = 70;
                }
                else if (RequestUser.CompanyId == (int)CompanyIds.SteelTech) {
                    logoLocation = steeltechLogo;
                }
                else if (RequestUser.CompanyId == (int)CompanyIds.Somico) {
                    logoLocation = somicoLogo;
                }
                Image logo = Image.GetInstance(Directory.GetCurrentDirectory() + logoLocation);
                logo.SetAbsolutePosition(20, (int)CurrentDoc.GetTop(logoPosition));

                // START HERE ----------------------------
                Paragraph Grid = new Paragraph("\n\n\n\n\n\n\n\n\n");
                Grid.Alignment = 1;
                Grid.SpacingAfter = 35f;
                document.Add(Grid);

                Paragraph Grid2 = new Paragraph();
                Grid2.Alignment = 1;
                Grid2.SpacingBefore = 35f;
                //Start Table -----------------------------------
                PdfPTable rightTable = new PdfPTable(7);
                int[] widths = { 1, 1, 1, 7, 2, 2, 2 };
                rightTable.SetWidths(widths);
                rightTable.TotalWidth = 570f;
                rightTable.LockedWidth = true;
                Font tableFont = new Font(Font.FontFamily.HELVETICA, 8);

                rightTable.HorizontalAlignment = Element.ALIGN_CENTER; 
                
                rightTable.AddCell(new Phrase("Item No.", tableFont)); 
                rightTable.AddCell(new Phrase("Quantity", tableFont));
                rightTable.AddCell(new Phrase("UOM", tableFont));
                rightTable.AddCell(new Phrase("Item Description", tableFont));
                rightTable.AddCell(new Phrase("Unit Price", tableFont));
                rightTable.AddCell(new Phrase("Discount", tableFont));
                rightTable.AddCell(new Phrase("Total Amount", tableFont));

                rightTable.HorizontalAlignment = Element.ALIGN_LEFT;

                int itemCount = 1;
                foreach (var poItem in PoItems) {
                    rightTable.AddCell(new Phrase(itemCount.ToString(), tableFont)); rightTable.AddCell(new Phrase(poItem.Quantity.ToString(), tableFont)); rightTable.AddCell(new Phrase(poItem.UOM, tableFont)); rightTable.AddCell(new Phrase(poItem.Description, tableFont)); rightTable.AddCell(new Phrase(poItem.Price.ToString(), tableFont)); rightTable.AddCell(new Phrase(poItem.Discount.ToString(), tableFont)); rightTable.AddCell(new Phrase(poItem.TotalPrice.ToString(), tableFont));
                    itemCount++;
                }
                //Nothing Follows
                rightTable.AddCell(new Phrase("", tableFont)); rightTable.AddCell(new Phrase("", tableFont)); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase("******** NOTHING FOLLOWS ********", tableFont)); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase(""));

                //Grand Total
                rightTable.AddCell(new Phrase("", tableFont)); rightTable.AddCell(new Phrase("", tableFont)); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase("", tableFont)); rightTable.AddCell(new Phrase("")); rightTable.AddCell(new Phrase("Grand Total:", new Font(Font.FontFamily.HELVETICA, 12))); rightTable.AddCell(new Phrase(PoItems.Sum(x => x.TotalPrice).ToString(), new Font(Font.FontFamily.HELVETICA, 12)));
                
                Grid2.Add(rightTable);
                document.Add(Grid2);
                // End of table -----------------------------------
                page = writer.DirectContentUnder;

                page.AddImage(logo);
                page.Stroke();

                // For "Purchase Order" Text
                PrintRectangle(20, 120, 570, 50);
                //For two tables
                PrintRectangle(20, 230, 350, 100);
                PrintRectangle(370, 230, 220, 100);
                //For Footer
                PrintRectangleBottom(20, 20, 570, 50);


                page.BeginText();

                SetFontSizeTo(24);
                
                PrintText("PURCHASE ORDER", 180, 105);

                SetPOCompanyHeader(RequestUser);

                SetFontSizeTo(10);
                // Header
                PrintText("To:", 25, 140); // Vendor Name

                SetFontSizeTo(8);
                PrintText(VendorName, 40, 140); // Vendor Name
                SetFontSizeTo(7);
                PrintText(VendorAddress, 25, 155); // Vendor Address
                SetFontSizeTo(10);

                PrintText("Vendor Code: " + VendorCode, 25, 220); // Venodr Id

                PrintText("PO Number: " + PoNumber, 375, 140); // Vendor Name
                PrintText("PO Date: " + DateTime.Now.ToShortDateString(), 375, 160); // Date Now
                PrintText("PR Number: " + RequestNumber, 375, 180); // Request Number
                PrintText("Delivery Date: " + DeliveryDate.ToShortDateString(), 375, 220); // Delivery Date


                //Footer
                SetFontSizeTo(8);
                PrintTextBottom("Purchase Orders are not valid unless signed by the Authorized Signatories. " + CurrentCompany + " will not recognize claims ", 20, 130);
                PrintTextBottom("based on verbal orders. If delivery is not made by the date indicated in this Purchase Order, United Steel Technology International Corporation may cancel this ", 20, 115);
                PrintTextBottom("Purchase Order, without any liability whatsoever. Seller expressly warrants that all goods or services furnished under this agreement shall conform to all ", 20, 100);
                PrintTextBottom("specifications and appropriate standards, will be new and will be free from defects in material or workmanship.", 20, 85);

                PrintTextBottom("Prepared By:", 25, 55);
                PrintTextBottom(CurrentUser.FirstName + " " + CurrentUser.LastName, 25, 30);
                PrintTextBottom("Noted By:", 255, 55);
                PrintTextBottom("Approved By:", 450, 55);

                //PrintTextBottom("No. of Days to Deliver:", 200, 65);
                //PrintTextBottom("Telephone No.:", 200, 50);
                //PrintTextBottom("Fax No.:", 200, 35);
                page.EndText();

                writer.Flush();
            }
            finally {
                document.Close();
            }
        }

        private void SetPOCompanyHeader(UserModel RequestUser) {
            SetFontSizeTo(8);
            if (RequestUser.CompanyName == "SONIC STEEL INDUSTRIES, INC.") {
                PrintText("SONIC STEEL INDUSTRIES, INC.", 375, 0);
                PrintText("8F Gedisco Tower, 534 Asuncion St., Binondo, Manila", 375, 10);
                PrintText("Tel. Nos. : 241-9251 to 58; Fax No. 241-7826", 375, 20);
                PrintText("Plant Address: Barangay Hugo Perez, Trece Martirez City", 375, 30);
                PrintText("Cavite", 375, 40);
                PrintText("TIN: 001-778-048-000 VAT", 375, 50);
            }
            else if (RequestUser.CompanyName == "UNITED STEEL TECHNOLOGY INT'L. CORP.") {
                PrintText("UNITED STEEL TECHNOLOGY INTERNATIONAL CORPORATION", 375, 0);
                PrintText("8F Gedisco Tower, 534 Asuncion St., Binondo, Manila", 375, 10);
                PrintText("Tel. Nos. : 241-9251 to 58; Fax No. 241-7826", 375, 20);
                PrintText("Plant Address: Km. 49 Indang Road, Barangay Inocencio", 375, 30);
                PrintText("Trece Martirez City, Cavite", 375, 40);
                PrintText("TIN: 007-106-703", 375, 50);
            }
            else if (RequestUser.CompanyName == "SOMICO STEEL MILL CORPORATION") {
                PrintText("SOMICO STEEL MILL CORPORATION", 375, 0);
                PrintText("8F Gedisco Tower, 534 Asuncion St., Binondo, Manila", 375, 10);
                PrintText("Tel. Nos. : 244-9293 to 96; Fax No. 241-7826", 375, 20);
                PrintText("Plant Address: Km. 29 Indang Road, Barangay Inocencio, ", 375, 30);
                PrintText("Trece Martirez City, Cavite", 375, 40);
            }
        }

        private void SetFontSizeTo(int Size) {
            page.SetFontAndSize(font, Size);
        }

        private void PrintText(string text, int x, int y) {
            page.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, (int)CurrentDoc.GetTop(y), 0);
        }

        private void PrintRectangle(int x, int y, float w, float h) {
            page.Rectangle(x, (int)CurrentDoc.GetTop(y), w, h);
            page.Stroke();
        }

        private void PrintRectangleBottom(int x, int y, float w, float h) {
            page.Rectangle(x, y, w, h);
            page.Stroke();
        }

        private void PrintTextRight(string text, int x, int y) {
            page.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, (int)CurrentDoc.GetRight(x), (int)CurrentDoc.GetTop(y), 0);
        }


        private void PrintTextBottom(string text, int x, int y) {
            page.ShowTextAligned(PdfContentByte.ALIGN_LEFT, text, x, y, 0);
        }


        private void PrintItem(string ItemNumber, string Description, string Quantity, ref int y) {
            int nextSpace = 12;
            PrintText(ItemNumber, 25, y);
            PrintText(Quantity, 375, y);
            if (Description.Length < 80) {
                PrintText(Description, 75, y);
                PrintText("_______________________________________________________________________________________________________________________", 20, y + 2);
            }
            else {
                nextSpace = 22;
                int middle = Description.Length / 2;
                PrintText(Description.Substring(0, middle), 75, y);
                PrintText(Description.Substring(middle, middle), 75, y + 10);
                PrintText("_______________________________________________________________________________________________________________________", 20, y + 12);
            }
            y += nextSpace;
        }

        private void PrintCanvasItem(string ItemNumber, string Description, string Quantity, string Uom, string Price1, string Price2, string Price3, ref int y) {
            int nextSpace = 12;
            PrintText(ItemNumber, 25, y);
            if (Description.Length < 80) {
                PrintText(Description, 75, y);
                PrintText("_______________________________________________________________________________________________________________________", 20, y + 2);
            }
            else {
                nextSpace = 22;
                int middle = Description.Length / 2;
                PrintText(Description.Substring(0, middle), 75, y);
                PrintText(Description.Substring(middle, middle), 75, y + 10);
                PrintText("_______________________________________________________________________________________________________________________", 20, y + 12);
            }
            PrintText(Quantity, 325, y);
            PrintText(Uom, 370, y);
            PrintText(Price1, 420, y);
            PrintText(Price2, 470, y);
            PrintText(Price3, 520, y);
            y += nextSpace;
        }
    }
}
