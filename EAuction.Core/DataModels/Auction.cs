using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public enum AuctionStatus { Active=1, Finished=2 }

    public class Auction
    {
        public Guid Id { get; set; }
        public string Description { get; set; }        
        public string ShippingAddress { get; set; }
        public string ShippingConditions { get; set; }
        public double MinRatingForParticipant { get; set; }
        public decimal StartPrice { get; set; }
        public decimal PriceStep { get; set; }
        public decimal MinPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public DateTime ? FinishDateAtActual { get; set; }
        public AuctionStatus Status { get; set; }

        public Guid AuctionTypeId { get; set; }
        public AuctionType AuctionType { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }        

        public ICollection<AuctionFile> AuctionFiles { get; set; }
        public ICollection<Bid> Bids { get; set; }
    }
}
