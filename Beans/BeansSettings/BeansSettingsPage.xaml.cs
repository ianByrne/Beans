using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
    public partial class BeansSettingsPage : Page, IBeansSettingsView
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

        public event EventHandler ImportCSVClicked;
        public event EventHandler LoadFromDbClicked;
        public event EventHandler SaveToDbClicked;
        public event EventHandler SelectedTransactionChanged;
        public event EventHandler TagChanged;
        public event EventHandler StartDateChanged;
        public event EventHandler EndDateChanged;
        public event EventHandler ViewLoaded;

        public BeansSettingsPage()
        {
            InitializeComponent();

            CtrlHeader.ImgLoading.Visibility = Visibility.Hidden;
            CtrlHeader.LblTitle.Content = "Beans: Settings";
        }

        public List<BeansTransaction> Transactions
        {
            get
            {
                var transactions = new List<BeansTransaction>();

                foreach (ItemCollection row in DgTransactions.Items)
                {
                    transactions.Add(new BeansTransaction
                    {
                        Date = Convert.ToDateTime(row[0].ToString()),
                        Name = row[1].ToString(),
                        Amount = Convert.ToDecimal(row[2])
                    });
                }

                return transactions;
            }
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    DgTransactions.Items.Clear();

                    foreach (var transaction in value)
                    {
                        var rowIndex = DgTransactions.Items.Add(new
                        {
                            Date = transaction.Date.ToString("yyyy-MM-dd"),
                            Name = transaction.Name,
                            Amount = transaction.Amount.ToString("F")
                        });

                        // Highlight any "unknowns"
                        if(transaction.Tags == null || transaction.Tags.Count == 0 || transaction.Tags.FirstOrDefault().Id == 0)
                        {
                            var row = (DataGridRow)DgTransactions.ItemContainerGenerator.ContainerFromIndex(0);

                            if (row != null)
                            {
                                row.Background = Brushes.Orange;
                            }
                        }
                    }
                });
            }
        }

        public BeansTransaction SelectedTransaction
        {
            get
            {
                var transaction = (dynamic)(DgTransactions.SelectedItem);

                if (transaction != null)
                {
                    return new BeansTransaction
                    {
                        Date = Convert.ToDateTime(transaction.Date),
                        Name = transaction.Name,
                        Amount = Convert.ToDecimal(transaction.Amount)
                    };
                }
                else
                {
                    return new BeansTransaction();
                }
            }
            set
            {
                //
            }
        }

        public DateTime StartDate
        {
            get
            {
                return DpkStartDate.SelectedDate.Value;
            }
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    DpkStartDate.Text = value.ToString("yyyy-MM-dd");
                });
            }
        }

        public DateTime EndDate
        {
            get
            {
                return DpkEndDate.SelectedDate.Value;
            }
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    DpkEndDate.Text = value.ToString("yyyy-MM-dd");
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

        public List<BeansTag> Tags
        {
            get
            {
                return new List<BeansTag>();
            }
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    CbxTags.Items.Clear();

                    foreach (var tag in value)
                    {
                        CbxTags.Items.Add(tag.Name);
                    }
                });
            }
        }

        public BeansTag SelectedTag
        {
            get
            {
                if (CbxTags.SelectedItem != null)
                {
                    return new BeansTag
                    {
                        Name = CbxTags.SelectedItem.ToString()
                    };
                }
                else
                {
                    return new BeansTag();
                }
            }
            set
            {
                // As we are changing UI elements from a different thread, we need to wrap the code in this Dispatcher.Invoke()
                this.Dispatcher.Invoke(() =>
                {
                    CbxTags.SelectedItem = value.Name;
                });
            }
        }

        private void PgeSettings_Loaded(object sender, EventArgs e)
        {
            if (ViewLoaded != null)
            {
                // Use Begin/EndInvoke() so as not to block main thread while querying DB
                ViewLoaded.BeginInvoke(this, EventArgs.Empty, Event_DoneInvoked, null);
            }
        }

        private void DpkStartDate_Changed(object sender, EventArgs e)
        {
            if (StartDateChanged != null)
            {
                StartDateChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void DpkEndDate_Changed(object sender, EventArgs e)
        {
            if (EndDateChanged != null)
            {
                EndDateChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnImportCSV_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files|*.csv";
            openFileDialog.Title = "Select a CSV file";
            if (openFileDialog.ShowDialog() == true)
            {
                if (ImportCSVClicked != null)
                {
                    ImportCSVClicked.BeginInvoke(openFileDialog.FileName, EventArgs.Empty, Event_DoneInvoked, null);
                }
            }
        }

        private void CbxTags_SelectionChanged(object sender, EventArgs e)
        {
            if (TagChanged != null)
            {
                TagChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void DgTransactions_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectedTransactionChanged != null)
            {
                SelectedTransactionChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnLoadFromDb_Click(object sender, EventArgs e)
        {
            if (LoadFromDbClicked != null)
            {
                // Use Begin/EndInvoke() so as not to block main thread while querying DB
                LoadFromDbClicked.BeginInvoke(this, EventArgs.Empty, Event_DoneInvoked, null);
            }
        }

        private void BtnSaveToDb_Click(object sender, EventArgs e)
        {
            if (SaveToDbClicked != null)
            {
                // Use Begin/EndInvoke() so as not to block main thread while querying DB
                SaveToDbClicked.BeginInvoke(this, EventArgs.Empty, Event_DoneInvoked, null);
            }
        }

        private void Event_DoneInvoked(IAsyncResult iar)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            var invokedMethod = (EventHandler)ar.AsyncDelegate;

            invokedMethod.EndInvoke(iar);
        }
    }
}
