using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EAuction.BLL.ViewModels
{
    public class EmployeeInfoViewModel
    {
        public string EmployeeId { get; set; }
        public string PositionName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DoB { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}