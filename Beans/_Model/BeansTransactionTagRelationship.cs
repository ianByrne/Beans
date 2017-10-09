using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beans
{
    public class BeansTransactionTagRelationship
    {
        public string TransactionName { get; set; }
        public List<BeansTag> Tags { get; set; }
    }
}
