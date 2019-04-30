using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class ApplicationUserPasswordHistory
    {
        public Guid Id { get; set; }
        public DateTime SetupDate { get; set; }
        public DateTime ? InvalidatedDate { get; set; }
        public string Password { get; set; }

        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
