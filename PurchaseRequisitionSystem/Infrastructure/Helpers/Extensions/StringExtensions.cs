using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Infrastructure.Helpers;
using Infrastructure.Security;
using System.Text.RegularExpressions;

namespace Infrastructure.Helpers.Extensions {
    public static class StringExtensions {
        public static TextBox ForceNumeric(this TextBox textBox) {
            int result;
            if (!Int32.TryParse(textBox.Text, out result))
            {
                textBox.Text = "1";
            }
            else
            {
                textBox.Text = result.ToString();
            }
            return textBox;
        }

        public static bool IsEmail(this string value) {
            string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
                        + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                        + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                        + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                        + @"[a-zA-Z]{2,}))$";
            Regex emailRegex = new Regex(patternStrict);
            return emailRegex.IsMatch(value);
        }

        public static bool IsNullOrEmpty(this string value) {
            if (value == null || value == String.Empty)
            {
                return true;
            }
            return false;
        }

        public static int ToInt(this string value) {
            if (value == null) {
                return 0;
            }
            try {
                return Convert.ToInt32(value);
            }
            catch {
                return 0;
            }
        }

        public static bool IsInteger(this string value) {
            int i;
            return int.TryParse(value, out i);
        }

        public static string WithTokens(this string value, params object[] tokens){
            return String.Format(value, tokens);
        }
    }
}
