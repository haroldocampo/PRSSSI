using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;

namespace Infrastructure.Print {
    public class PrintManager {
        public void Print(string text) {
            FlowDocument document = new FlowDocument(new Paragraph(new Run(text)));

            PrintDialog printDialog = new PrintDialog();

            printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "");
        }
    }
}
