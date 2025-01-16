using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Model
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public string DebtSource { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? Date { get; set; }
        public string Note { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public bool? DebtCleared { get; set; }
        public string CreatedBy { get; set; }


    }

}
