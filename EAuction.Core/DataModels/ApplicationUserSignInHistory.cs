using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class ApplicationUserSignInHistory
    {
        public Guid Id { get; set; }
        public DateTime SignInTime { get; set; }
        public string MachineIp { get; set; }
        public string IpToGeoCountry { get; set; }
        public string IpToGeoCity { get; set; }
        public double IpToGeoLatitude { get; set; }
        public double IpToGeoLongitude { get; set; }

        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
