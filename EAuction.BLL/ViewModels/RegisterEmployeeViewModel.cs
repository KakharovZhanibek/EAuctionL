using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.BLL.ViewModels
{
    public class RegisterEmployeeViewModel
    {
        public Guid EmployeePositionId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DoB { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
