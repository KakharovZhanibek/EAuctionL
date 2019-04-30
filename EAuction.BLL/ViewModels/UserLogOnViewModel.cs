using EAuction.BLL.ExternalModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EAuction.BLL.ViewModels
{
    public class UserLogOnViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public GeoLocationInfo GeoLocation { get; set; }
    }
}