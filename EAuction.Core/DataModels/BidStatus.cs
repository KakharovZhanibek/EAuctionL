using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class BidStatus
    {
        public Guid Id { get; set; }
        public string StatusName { get; set; }

        public ICollection<Bid> Bids { get; set; }
    }
}
