using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class OrganizationRating
    {
        public Guid Id { get; set; }
        public double Score { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
