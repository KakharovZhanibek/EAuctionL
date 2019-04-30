using EAuction.BLL.ExternalModels;
using EAuction.BLL.ViewModels;
using EAuction.Core.DataModels;
using EAuction.Infrastructure;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace EAuction.BLL.Services
{
    public class UserManagementService
    {
        private readonly IdentityDbContext _identityDbContext;

        public bool IsValidUser(UserLogOnViewModel model)
        {
            model.GeoLocation = GeoLocationInfo.GetGeolocationInfo();

            var user = _identityDbContext.ApplicationUsers.Include("ApplicationUserPasswordHistories")
                .SingleOrDefault(p => p.Email == model.Email);
            if (user == null)
                throw new Exception($"Пользователя с email {model.Email} нет в базе");

            var userPassword = user.ApplicationUserPasswordHistories.SingleOrDefault(p => p.Password == model.Password);
            if (userPassword==null)
            {
                user.FailedSignInCount+=1;
                _identityDbContext.SaveChanges();
                throw new Exception("Неверный пароль");
            }
            if (userPassword!=null && userPassword.InvalidatedDate!=null)
            {
                user.FailedSignInCount += 1;
                _identityDbContext.SaveChanges();
                throw new Exception("Аккаунт пользователя заблокирован");
            }

            ApplicationUserSignInHistory userSignInHistory = new ApplicationUserSignInHistory()
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = user.Id,
                SignInTime = DateTime.Now,
                MachineIp = model.GeoLocation.ip,
                IpToGeoCountry = model.GeoLocation.country_name,
                IpToGeoCity = model.GeoLocation.city,
                IpToGeoLatitude = model.GeoLocation.latitude,
                IpToGeoLongitude = model.GeoLocation.longitude
            };
            _identityDbContext.ApplicationUserSignInHistories.Add(userSignInHistory);
            _identityDbContext.SaveChanges();

            return true;
        }

        public bool IsUserFarFromLast5SignIn(string userId) 
        {           
            var userSignInHistory = _identityDbContext.ApplicationUserSignInHistories
                .Where(p => p.ApplicationUserId.ToString() == userId).ToList();
            if (userSignInHistory.Count == 0)
                throw new Exception($"У пользователя {userId} нет истории входов в базе");

            var currentSignIn = userSignInHistory
                .OrderByDescending(p => p.SignInTime)
                .SingleOrDefault();

            var userPrev5SignIn = userSignInHistory
                .Where(p=>p.SignInTime!= currentSignIn.SignInTime)
                .OrderByDescending(p=>p.SignInTime)
                .ToList();

            if (userPrev5SignIn.Count == 0)
                throw new Exception($"У пользователя {userId} нет истории предыдущих входов в базе");

            double currentLatitude = currentSignIn.IpToGeoLatitude;
            double currentLongitude = currentSignIn.IpToGeoLongitude;

            GeoCoordinate currentCoordinate = new GeoCoordinate(currentLatitude, currentLongitude);
            
            int SignInCount = 1;            
            foreach (ApplicationUserSignInHistory item in userPrev5SignIn)
            {
                if (SignInCount >= 5)
                {
                    break;
                }

                double latitude = item.IpToGeoLatitude;
                double longitude = item.IpToGeoLongitude;

                GeoCoordinate tmp = new GeoCoordinate(latitude, longitude);

                if (currentCoordinate.GetDistanceTo(tmp) / 1000 > 2000)
                {
                    return true;
                }
                SignInCount++;
            }

            return false;
        }


        public int SendSmsCodeToUser(string userPhone)
        {
            const string accountSid = "DhKmyhfjJRm96ghnM23dfg11";
            const string authToken = "jr5fgyhj2h4645541h521xdk";
            Random rnd = new Random();
            int smsCode = rnd.Next(1000, 9999);

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: $"Enter this code {smsCode}",
                from: new Twilio.Types.PhoneNumber(/*Tel*/""),
                to: new Twilio.Types.PhoneNumber(userPhone)
            );
            return smsCode;
        }

        public void MandatoryUserPasswordChange(string userId)
        {
            var userSignInHistory = _identityDbContext.ApplicationUserSignInHistories
               .Where(p => p.ApplicationUserId.ToString() == userId).ToList();
            if (userSignInHistory.Count == 0)
                throw new Exception($"У пользователя {userId} нет истории входов в базе");

            var currentPasswordSetupDate = _identityDbContext.ApplicationUserPasswordHistories
                .SingleOrDefault(p => p.ApplicationUserId.ToString() == userId && p.InvalidatedDate == null);
            if (currentPasswordSetupDate == null)
                throw new Exception($"Аккаунт пользователя {userId} не активен");

            var userSignInCountWithCurrentPass = userSignInHistory.Where(p => p.SignInTime >= currentPasswordSetupDate.SetupDate).ToList();
            if (userSignInCountWithCurrentPass.Count>50)
                throw new Exception($"Пользователю {userId} необходимо сменить пароль");
        }

        public static UserLogOnViewModel UserInfoForRegistration()
        {
            UserLogOnViewModel model = new UserLogOnViewModel();

            Console.WriteLine("Введите email");
            model.Email = Console.ReadLine();

            Console.WriteLine("Введите пароль");
            model.Password = Console.ReadLine();

            return model;
        }

        public UserManagementService()
        {
            _identityDbContext = new IdentityDbContext();
        }
    }
}