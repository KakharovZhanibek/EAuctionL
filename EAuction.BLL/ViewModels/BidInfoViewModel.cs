using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EAuction.BLL.ViewModels
{
    public class BidInfoViewModel
    {
        public string BidId { get; set; }
        public string AuctionId { get; set; }
        public string AuctionType { get; set; }
        public string AuctionDescription { get; set; }
        public string BidStatus { get; set; }
        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public string BidDescription { get; set; }        
    }
}