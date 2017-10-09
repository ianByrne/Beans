using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Beans
{
    public interface IBeansReportsView
    {
        // View Elements
        List<BeansExpenseType> ExpenseTypes { get; set; }
        bool ShowLoadingImage { get; set; }
        string DateRange { set; }

        // View Events
        event RoutedEventHandler ViewLoaded;

        // Nav
        event EventHandler ReportsClicked;
        event EventHandler SettingsClicked;
    }
}
