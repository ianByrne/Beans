using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace Beans._Model
{
    public static class BankStatementParser
    {
        private enum BankType
        {
            Lloyds,
            Amex
        }

        public static List<BeansTransaction> ParseCSVFromFile(string filePath)
        {
            BankType bankType;
            DataTable dt = new DataTable();

            using (StreamReader sr = new StreamReader(filePath, Encoding.Default))
            {
                // If it's Amex, it starts with a comma and will have a bunch of header guff
                var firstChar = Convert.ToChar(sr.Peek());
                if (firstChar == ',')
                {
                    bankType = BankType.Amex;

                    // Skip past the header guff
                    for (int i = 0; i < 20; i++)
                    {
                        var thisLine = Convert.ToChar(sr.Peek());
                        if (thisLine == 'D')
                            break;
                        else
                            sr.ReadLine();
                    }
                }
                else
                {
                    bankType = BankType.Lloyds;
                }

                string[] headers = sr.ReadLine().Split(',');

                // Remove any empty headers (stupid Lloyds)
                headers = headers.Where(header => !string.IsNullOrEmpty(header)).ToArray<string>();

                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = System.Text.RegularExpressions.Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }

            List<BeansTransaction> transactions = new List<BeansTransaction>();

            foreach (DataRow row in dt.Rows)
            {
                switch (bankType)
                {
                    case BankType.Amex:
                        try
                        {
                            // Stupid bloody pound signs and commas
                            var amount = row["Amount"].ToString();
                            amount = amount.Replace("£", "");
                            amount = amount.Replace(",", "");

                            transactions.Add(new BeansTransaction
                            {
                                Date = DateTime.ParseExact(row["Date"].ToString(), "dd MMM yyyy", new System.Globalization.CultureInfo("en-GB")),
                                Name = row["Description"].ToString(),
                                Amount = Convert.ToDecimal(amount)
                            });
                        }
                        catch { }
                        break;

                    case BankType.Lloyds:
                        try
                        {
                            transactions.Add(new BeansTransaction
                            {
                                Date = DateTime.ParseExact(row["Transaction Date"].ToString(), "dd/MM/yyyy", new System.Globalization.CultureInfo("en-GB")),
                                Name = row["Transaction Description"].ToString(),
                                Amount = Convert.ToDecimal(row["Debit Amount"].ToString())
                            });
                        }
                        catch { }
                        break;
                }
            }

            return transactions;
        }
    }
}
