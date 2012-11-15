using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Infrastructure.Helpers.Extensions {
    public static class ControlExtensions {
        public static Control FlagAsError(this Control control) {
            control.BorderBrush = new SolidColorBrush() { Color = Color.FromRgb(250, 0, 0) };
            return control;
        }

        public static Control FlagAsCorrect(this Control control) {
            control.BorderBrush = new SolidColorBrush() { Color = Color.FromRgb(0, 250, 0) };
            return control;
        }

        public static Control FlagAsNormal(this Control control) {
            control.BorderBrush = new SolidColorBrush() { Color = Color.FromRgb(0, 0, 0) };
            return control;
        }
    }
}
