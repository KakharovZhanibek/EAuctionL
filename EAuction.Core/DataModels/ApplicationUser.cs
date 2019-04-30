using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class ApplicationUser
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int FailedSignInCount { get; set; }
        public Guid AssosiatedEmployeeId { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<ApplicationUserPasswordHistory> ApplicationUserPasswordHistories { get; set; }
        public ICollection<ApplicationUserSignInHistory> ApplicationUserSignInHistories { get; set; }
    }
}
