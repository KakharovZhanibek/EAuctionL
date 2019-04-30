using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }        
        public string IdentificationNumber { get; set; }        
        public string Address { get; set; }
        public string Email { get; set; }
        public string Contacts { get; set; }
        public string Site { get; set; }
        public DateTime ? RegistrationDate { get; set; }

        public Guid OrganizationTypeId { get; set; }
        public OrganizationType OrganizationType { get; set; }

        public ICollection<Auction> Auctions { get; set; }
        public ICollection<OrganizationFile> OrganizationFiles { get; set; }
        public ICollection<OrganizationRating> OrganizationRatings { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public ICollection<Bid> Bids { get; set; }        
        public ICollection<Transaction> Transactions { get; set; }
    }
}
