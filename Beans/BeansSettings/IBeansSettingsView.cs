using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Beans
{
    public interface IBeansSettingsView
    {
        // View Elements
        bool ShowLoadingImage { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        List<BeansTransaction> Transactions { get; set; }
        List<BeansTag> Tags { get; set; }
        BeansTag SelectedTag { get; set; }
        BeansTransaction SelectedTransaction { get; set; }

        // View Events
        event EventHandler ViewLoaded;
        event EventHandler StartDateChanged;
        event EventHandler EndDateChanged;
        event EventHandler SelectedTransactionChanged;
        event EventHandler TagChanged;
        event EventHandler ImportCSVClicked;
        event EventHandler LoadFromDbClicked;
        event EventHandler SaveToDbClicked;

        // Nav
        event EventHandler ReportsClicked;
        event EventHandler SettingsClicked;
    }
}
