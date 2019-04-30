using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EAuction.BLL.ViewModels
{
    public class TransactionInfoViewModel
    {
        public string TransactionId { get; set; }
        public string TransactionTypeName { get; set; }
        public decimal Sum { get; set; }        
        public string Description { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
    }
}