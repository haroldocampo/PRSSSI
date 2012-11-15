using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Infrastructure.Helpers.Extensions {
    public static class DataGridExtensions {
        public static DataGrid ClearFirstColumn(this DataGrid dataGrid) {
            if (dataGrid.Items.Count <= 0) return dataGrid;
            dataGrid.Columns[0].Visibility = Visibility.Collapsed;
            return dataGrid;
        }
    }
}
