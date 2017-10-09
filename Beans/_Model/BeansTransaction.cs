using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beans
{
    public class BeansTransaction
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public List<BeansTag> Tags { get; set; }
    }
}
