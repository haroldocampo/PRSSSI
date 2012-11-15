using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace PurchaseRequisitionSystem.Pages.CS {
    public class ZeroConverter : IValueConverter{
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double number;
            try {
                number = (double)value;
            }
            catch(InvalidCastException) {
                return false;
            }

            if (number == 0) {
                return false;
            }
            else {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
