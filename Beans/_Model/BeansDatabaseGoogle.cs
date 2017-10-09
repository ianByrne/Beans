using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Beans
{
    public class BeansDatabaseGoogle : IBeansDatabase
    {
        private readonly GoogleSheet _googleSheet;
        private List<BeansTransaction> _transactions;
        private List<BeansTag> _tags;
        private List<BeansTransactionTagRelationship> _relationships;

        public BeansDatabaseGoogle(GoogleSheet googleSheet)
        {
            _googleSheet = googleSheet;
        }

        /***********************************
         * TRANSACTIONS
         * ********************************/
        public async Task<List<BeansTransaction>> GetTransactions(DateTime? startDate, DateTime? endDate)
        {
            await GetTagsFromDB();
            await GetTransactionTagRelationshipsFromDB();
            await GetTransactionsFromDB(startDate, endDate);

            return _transactions;
        }

        public BeansTransaction PopulateTags(BeansTransaction transaction)
        {
            var thisRelationship = _relationships.Where(relationship => relationship.TransactionName == transaction.Name).FirstOrDefault();

            if (thisRelationship != null)
            {
                transaction.Tags = thisRelationship.Tags;
            }
            else
            {
                transaction.Tags = new List<BeansTag>
                {
                    new BeansTag
                    {
                        Id = 0,
                        Name = "Unknown"
                    }
                };
            }

            return transaction;
        }

        public List<BeansTransaction> PopulateTags(List<BeansTransaction> transactions)
        {
            List<BeansTransaction> transactionsWithTags = new List<BeansTransaction>();

            foreach (var transaction in transactions)
            {
                transactionsWithTags.Add(PopulateTags(transaction));
            }

            return transactionsWithTags;
        }

        private async Task GetTransactionsFromDB(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Check connection
            if(_googleSheet.Service == null)
            {
                await _googleSheet.Connect();
            }

            List<BeansTransaction> transactions = new List<BeansTransaction>();

            string range = "Transactions!A:D";
            SpreadsheetsResource.ValuesResource.GetRequest request = _googleSheet.Service.Spreadsheets.Values.Get(_googleSheet.SpreadsheetId, range);

            ValueRange response = request.ExecuteAsync().Result;
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    BeansTransaction thisTransaction = new BeansTransaction();

                    if (row.Count > 0)
                    {
                        thisTransaction.Date = DateTime.Parse(row[0].ToString());
                    }

                    if (row.Count > 1)
                    {
                        var name = row[1].ToString();
                        if (name.Contains("UBER "))
                            name = "UBER";

                        thisTransaction.Name = name;
                    }

                    if (row.Count > 2)
                    {
                        string amountCell = row[2].ToString();

                        if (!string.IsNullOrEmpty(amountCell))
                        {
                            thisTransaction.Amount = Convert.ToDecimal(row[2].ToString());
                        }
                    }

                    if (row.Count > 3)
                    {
                        thisTransaction.Notes = row[3].ToString();
                    }

                    // Ensure this transaction falls between the start/end dates (not great, I know)
                    if (startDate != null && endDate != null)
                    {
                        if (thisTransaction.Date >= startDate && thisTransaction.Date <= endDate)
                        {
                            transactions.Add(thisTransaction);
                        }
                    }
                    else
                    {
                        transactions.Add(thisTransaction);
                    }
                }

                // Populate all of the Tags
                transactions = PopulateTags(transactions);
            }
            else
            {
                // No data found
            }

            _transactions = transactions;
        }

        public async Task WriteTransactions(List<BeansTransaction> transactions)
        {
            // Check connection
            if (_googleSheet.Service == null)
            {
                await _googleSheet.Connect();
            }

            // Refresh transactions from the DB and add them to the list
            await GetTransactionsFromDB();
            _transactions.AddRange(transactions);

            // Remove duplicates and order ascending
            _transactions = _transactions
                .GroupBy(transaction => new { transaction.Date, transaction.Name, transaction.Amount })
                .Select(transactionGroup => transactionGroup.FirstOrDefault())
                .OrderBy(transaction => transaction.Date)
                .ToList();

            string range = "Transactions!A:D";
            ValueRange valueRange = new ValueRange();
            IList<IList<object>> rows = new List<IList<object>>();

            foreach (var transaction in _transactions)
            {
                IList<object> cells = new List<object>();
                cells.Add(transaction.Date.ToString("yyyy-MM-dd"));
                cells.Add(transaction.Name);
                cells.Add(transaction.Amount);
                cells.Add(transaction.Notes);
                rows.Add(cells);
            }

            valueRange.Values = rows;

            SpreadsheetsResource.ValuesResource.UpdateRequest request = _googleSheet.Service.Spreadsheets.Values.Update(valueRange, _googleSheet.SpreadsheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            var response = await request.ExecuteAsync();
        }

        /***********************************
         * TAGS
         * ********************************/
        public async Task<List<BeansTag>> GetTags()
        {
            return _tags;
        }

        private async Task GetTagsFromDB()
        {
            // Check connection
            if (_googleSheet.Service == null)
            {
                await _googleSheet.Connect();
            }

            List<BeansTag> tags = new List<BeansTag>();

            string range = "Tags!A:B";
            SpreadsheetsResource.ValuesResource.GetRequest request = _googleSheet.Service.Spreadsheets.Values.Get(_googleSheet.SpreadsheetId, range);

            ValueRange response = request.ExecuteAsync().Result;
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    BeansTag thisTag = new BeansTag();

                    if (row.Count > 0)
                    {
                        thisTag.Id = Convert.ToInt32(row[0].ToString());
                    }

                    if (row.Count > 1)
                    {
                        thisTag.Name = row[1].ToString();
                    }

                    tags.Add(thisTag);
                }
            }
            else
            {
                // No data found
            }

            _tags = tags;
        }

        //private async Task WriteTags(List<BeansTag> tags)
        //{
        //    //
        //}

        /***********************************
         * TRANSACTION-TAG RELATIONSHIPS
         * ********************************/
        public async Task<List<BeansTransactionTagRelationship>> GetTransactionTagRelationships()
        {
            return _relationships;
        }

        private async Task GetTransactionTagRelationshipsFromDB()
        {
            // Check connection
            if (_googleSheet.Service == null)
            {
                await _googleSheet.Connect();
            }

            List<BeansTransactionTagRelationship> relationships = new List<BeansTransactionTagRelationship>();

            string range = "TransactionTagRelationships!A:B";
            SpreadsheetsResource.ValuesResource.GetRequest request = _googleSheet.Service.Spreadsheets.Values.Get(_googleSheet.SpreadsheetId, range);

            ValueRange response = request.ExecuteAsync().Result;
            IList<IList<Object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    BeansTransactionTagRelationship thisRelationship = new BeansTransactionTagRelationship();
                    string tagIdsCSV = "";

                    if (row.Count > 0)
                    {
                        thisRelationship.TransactionName = row[0].ToString();
                    }

                    if (row.Count > 1)
                    {
                        // Get the CSV value of the Tag Ids from the spreadsheet
                        tagIdsCSV = row[1].ToString();
                    }

                    // Skip any relationship that's not got any IDs
                    if (string.IsNullOrEmpty(tagIdsCSV))
                        continue;

                    // Convert the CSV string to a list of Int IDs
                    var tagIdsListInt = tagIdsCSV.Split(',').Select(int.Parse).ToList();

                    // Loop through the Int IDs and add them to a list of BeansTag
                    List<BeansTag> theseTags = new List<BeansTag>();
                    foreach (var tagId in tagIdsListInt)
                    {
                        var thisTag = _tags.Where(tag => tag.Id == tagId).FirstOrDefault();
                        theseTags.Add(thisTag);
                    }

                    thisRelationship.Tags = theseTags;
                    relationships.Add(thisRelationship);
                }
            }
            else
            {
                // No data found
            }

            _relationships = relationships;
        }

        public async Task WriteTransactionTagRelationships(List<BeansTransactionTagRelationship> relationships)
        {
            // Check connection
            if (_googleSheet.Service == null)
            {
                await _googleSheet.Connect();
            }

            // Refresh relationships from the DB and add them to the list
            await GetTransactionTagRelationshipsFromDB();
            _relationships.AddRange(relationships);

            // Remove duplicates and order ascending
            _relationships = _relationships
                .GroupBy(relationship => relationship.TransactionName)
                .Select(relationshipGroup =>
                {
                    // If there are duplicate relationships, trump in order of passed relationships, then any with a tag
                    if (relationshipGroup.Count() > 1)
                    {
                        var relationshipsWithTags = relationshipGroup.Where(thisRelationship => thisRelationship.Tags.Count > 0);
                        if (relationshipsWithTags.Count() > 1)
                        {
                            // Use the relationship that was passed
                            return relationshipsWithTags
                                .FirstOrDefault(thisRelationship => thisRelationship == relationships
                                    .FirstOrDefault(passedRelationship => passedRelationship.TransactionName == thisRelationship.TransactionName));
                        }
                        else
                        {
                            return relationshipGroup.FirstOrDefault();
                        }
                    }
                    else
                    {
                        return relationshipGroup.FirstOrDefault();
                    }
                })
                .OrderBy(relationship => relationship.TransactionName)
                .ToList();

            string range = "TransactionTagRelationships!A:B";
            ValueRange valueRange = new ValueRange();
            IList<IList<object>> rows = new List<IList<object>>();

            foreach (var relationship in _relationships)
            {
                IList<object> cells = new List<object>();
                cells.Add(relationship.TransactionName);
                if (relationship.Tags != null)
                {
                    var tags = relationship.Tags.Select(tag => tag.Id);
                    cells.Add(string.Join(",", tags));
                }
                else
                {
                    cells.Add("");
                }
                rows.Add(cells);
            }

            valueRange.Values = rows;

            SpreadsheetsResource.ValuesResource.UpdateRequest request = _googleSheet.Service.Spreadsheets.Values.Update(valueRange, _googleSheet.SpreadsheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            var response = await request.ExecuteAsync();
        }
    }
}
