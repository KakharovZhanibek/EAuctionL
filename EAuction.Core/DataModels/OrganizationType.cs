using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class OrganizationType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<Organization> Organizations { get; set; }
    }
}
