using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beans
{
    public interface IBeansDatabase
    {
        // Transactions
        Task<List<BeansTransaction>> GetTransactions(DateTime? startDate, DateTime? endDate);
        BeansTransaction PopulateTags(BeansTransaction transaction);
        List<BeansTransaction> PopulateTags(List<BeansTransaction> transactions);
        Task WriteTransactions(List<BeansTransaction> transactions);

        // Tags
        Task<List<BeansTag>> GetTags();
        //void WriteTags(List<BeansTag> tags);

        // TransactionTagRelationships
        Task<List<BeansTransactionTagRelationship>> GetTransactionTagRelationships();
        Task WriteTransactionTagRelationships(List<BeansTransactionTagRelationship> relationships);
    }
}
