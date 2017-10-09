using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Beans
{
    public sealed class BeansReportsPresenter
    {
        private IBeansReportsView _view;

        public BeansReportsPresenter(IBeansReportsView view)
        {
            _view = view;

            // Events
            _view.ViewLoaded += new RoutedEventHandler(ViewLoaded);
            _view.SettingsClicked += new EventHandler(SettingsClicked);
        }

        private async Task PopulateReports()
        {
            // Put up the loading message
            _view.ShowLoadingImage = true;

            // Update the view DateRange to be used in the title
            _view.DateRange = $"{Utils.StartDate.ToString("d MMM")} to {Utils.EndDate.ToString("d MMM")}";

            // Query DB
            var transactions = await Utils.BeansDatabase.GetTransactions(Utils.StartDate, Utils.EndDate);

            // Group the transactions by their tags
            List<BeansExpenseType> expenseTypes = transactions
                .Where(transaction => transaction.Tags.Count > 0) // Only use those that have a tag
                .GroupBy(transaction => transaction.Tags.FirstOrDefault().Name) // Group them by the first tag they have (even though database supports multiple tags per transaction, nothing else does yet)
                .Select(transactionGrouping => // Format the groups into a list of BeansExpenseType with their amounts summed
                {
                    string groupName = transactionGrouping.Key.ToString();
                    decimal groupAmount = transactionGrouping.Sum(transaction => transaction.Amount);

                    return new BeansExpenseType
                    {
                        Name = groupName,
                        Amount = groupAmount
                    };
                })
                .Where(expenseType => expenseType.Name != "Ignore") // Don't include the ignore list
                .OrderBy(expenseType => expenseType.Name) // Order by category name
                .ToList();

            // Add the total
            decimal total = expenseTypes.Sum(expenseType => expenseType.Amount);

            expenseTypes.Add(new BeansExpenseType
            {
                Name = "TOTAL",
                Amount = total
            });

            // Hide the loading message
            _view.ShowLoadingImage = false;

            // Output to the view
            _view.ExpenseTypes = expenseTypes;
        }

        /* EVENTS */
        private void SettingsClicked(object sender, EventArgs e)
        {
            Utils.BeansMainWindow.NavigationService.Navigate(Utils.SettingsView);
        }

        private async void ViewLoaded(object sender, EventArgs e)
        {
            await PopulateReports();
        }
    }
}
