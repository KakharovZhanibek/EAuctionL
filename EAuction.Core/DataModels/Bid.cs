using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class Bid
    {
        public Guid Id { get; set; }        
        public decimal Price { get; set; }        
        public DateTime CreatedDate { get; set; }
        public string Description { get; set; }
        public bool ? IsWin { get; set; }

        public Guid BidStatusId { get; set; }
        public BidStatus BidStatus { get; set; }

        public Guid AuctionId { get; set; }
        public Auction Auction { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

    }
}
