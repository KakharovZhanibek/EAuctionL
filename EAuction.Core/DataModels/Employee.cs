using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Core.DataModels
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DoB { get; set; }
        public string Email { get; set; }

        public Guid EmployeePositionId { get; set; }
        public EmployeePosition EmployeePosition { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
