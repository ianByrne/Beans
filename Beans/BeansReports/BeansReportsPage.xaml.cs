using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Beans
{
    public partial class BeansReportsPage : Page, IBeansReportsView
    {
        public event EventHandler SettingsClicked
        {
            add { CtrlHeader.SettingsClicked += value; }
            remove { CtrlHeader.SettingsClicked -= value; }
        }

        public event EventHandler ReportsClicked
        {
            add { CtrlHeader.ReportsClicked += value; }
            remove { CtrlHeader.ReportsClicked -= value; }
        }

        public event RoutedEventHandler ViewLoaded;

        public BeansReportsPage()
        {
            InitializeComponent();
        }

        public List<BeansExpenseType> ExpenseTypes
        {
            get
            {
                var expenseTypes = new List<BeansExpenseType>();

                foreach(ItemCollection row in DgExpenseTypes.Items)
                {
                    expenseTypes.Add(new BeansExpenseType
                    {
                        Name = row[0].ToString(),
                        Amount = Convert.ToDecimal(row[1])
                    });
                }

                return expenseTypes;
            }
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    DgExpenseTypes.Items.Clear();

                    foreach (var expenseType in value)
                    {
                        DgExpenseTypes.Items.Add(new BeansExpenseType
                        {
                            Name = expenseType.Name,
                            Amount = expenseType.Amount
                        });
                    }
                });
            }
        }

        public string DateRange
        {
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    CtrlHeader.LblTitle.Content = $"Beans: {value}";
                });
            }
        }

        public bool ShowLoadingImage
        {
            get
            {
                return CtrlHeader.ImgLoading.IsVisible;
            }
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    if (value == true)
                    {
                        CtrlHeader.ImgLoading.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        CtrlHeader.ImgLoading.Visibility = Visibility.Hidden;
                    }
                });
            }
        }

        private void PgeReports_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewLoaded != null)
            {
                // Use Begin/EndInvoke() so as not to block main thread while querying DB
                ViewLoaded.BeginInvoke(this, e, Event_DoneInvoked, null);
            }
        }

        private void Event_DoneInvoked(IAsyncResult iar)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (RoutedEventHandler)ar.AsyncDelegate;

            invokedMethod.EndInvoke(iar);
        }
    }
}
