using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public enum TransactionType { Deposit, Withdraw }
    public class Transaction
    {
        public Guid Id { get; set; }        
        public TransactionType TransactionType { get; set; }
        public decimal Sum { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
