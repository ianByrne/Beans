using Beans._Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Beans
{
    public sealed class BeansSettingsPresenter
    {
        private IBeansSettingsView _view;
        private List<BeansTransaction> _transactions { get; set; }
        private List<BeansTransactionTagRelationship> _relationships { get; set; }

        public BeansSettingsPresenter(IBeansSettingsView view)
        {
            _view = view;

            // Events
            _view.ViewLoaded += new EventHandler(ViewLoaded);
            _view.ReportsClicked += new EventHandler(ReportsClicked);

            _view.ImportCSVClicked += new EventHandler(ImportCSVClicked);
            _view.LoadFromDbClicked += new EventHandler(LoadFromDbClicked);
            _view.SaveToDbClicked += new EventHandler(SaveToDbClicked);
            _view.StartDateChanged += new EventHandler(StartDateChanged);
            _view.EndDateChanged += new EventHandler(EndDateChanged);
            _view.TagChanged += new EventHandler(TagChanged);
            _view.SelectedTransactionChanged += new EventHandler(SelectedTransactionChanged);
        }

        private async Task PopulateTags()
        {
            var tags = await Utils.BeansDatabase.GetTags();
            _view.Tags = tags;
        }

        private async Task PopulateFromDb()
        {
            // Put up the loading message
            _view.ShowLoadingImage = true;

            // Update the datepicker dates
            _view.StartDate = Utils.StartDate;
            _view.EndDate = Utils.EndDate;

            // Query the db for the date range
            _transactions = await Utils.BeansDatabase.GetTransactions(Utils.StartDate, Utils.EndDate);
            _relationships = await Utils.BeansDatabase.GetTransactionTagRelationships();

            // Output to the view
            _view.Transactions = _transactions;

            // Hide the loading message
            _view.ShowLoadingImage = false;
        }

        private async Task PopulateFromCSV(string fileName)
        {
            // Put up the loading message
            _view.ShowLoadingImage = true;

            _transactions = BankStatementParser.ParseCSVFromFile(fileName);
            _view.Transactions = _transactions;

            // TO DO: Update the start/end dates with the CSV dates?

            // Hide the loading message
            _view.ShowLoadingImage = false;
        }

        private async Task SaveTransactionsToDb()
        {
            // Put up the loading message
            _view.ShowLoadingImage = true;

            await Utils.BeansDatabase.WriteTransactions(_transactions);

            // Hide the loading message
            _view.ShowLoadingImage = false;
        }

        private async Task SaveRelationshipsToDb()
        {
            // Put up the loading message
            _view.ShowLoadingImage = true;

            await Utils.BeansDatabase.WriteTransactionTagRelationships(_relationships);

            // Hide the loading message
            _view.ShowLoadingImage = false;
        }

        /* EVENTS */
        private void ReportsClicked(object sender, EventArgs e)
        {
            Utils.BeansMainWindow.NavigationService.Navigate(Utils.ReportsView);
        }

        private async void ViewLoaded(object sender, EventArgs e)
        {
            await PopulateFromDb();
            await PopulateTags();
        }

        private async void ImportCSVClicked(object sender, EventArgs e)
        {
            string fileName = sender.ToString();
            await PopulateFromCSV(fileName);
        }

        private async void LoadFromDbClicked(object sender, EventArgs e)
        {
            await PopulateFromDb();
        }

        private async void SaveToDbClicked(object sender, EventArgs e)
        {
            await SaveTransactionsToDb();
            await SaveRelationshipsToDb();

            await PopulateFromDb();
        }

        private async void TagChanged(object sender, EventArgs e)
        {
            BeansTransaction transaction = _view.SelectedTransaction;
            BeansTag tag = _view.SelectedTag;

            if (transaction != null && tag.Name != null)
            {
                // The tag we get back is missing the id. Go and find it.
                var tags = await Utils.BeansDatabase.GetTags();
                tag.Id = tags.FirstOrDefault(thisTag => thisTag.Name == tag.Name).Id;

                // Update the relationship with the new tag
                var thisRelationship = _relationships.FirstOrDefault(relationship => relationship.TransactionName == transaction.Name);

                if (thisRelationship != null)
                {
                    thisRelationship.Tags = new List<BeansTag> { tag };
                }
                else
                {
                    _relationships.Add(new BeansTransactionTagRelationship
                    {
                        TransactionName = transaction.Name,
                        Tags = new List<BeansTag> { new BeansTag { Id = 0, Name = "Unknown" } }
                    });
                }
            }
        }

        private void SelectedTransactionChanged(object sender, EventArgs e)
        {
            // Update the tags combobox to select the correct one
            BeansTransaction transaction = _view.SelectedTransaction;
            transaction = Utils.BeansDatabase.PopulateTags(transaction);

            _view.SelectedTag = transaction.Tags.FirstOrDefault();
        }

        private void StartDateChanged(object sender, EventArgs e)
        {
            Utils.StartDate = _view.StartDate;
        }

        private void EndDateChanged(object sender, EventArgs e)
        {
            Utils.EndDate = _view.EndDate;
        }
    }
}
