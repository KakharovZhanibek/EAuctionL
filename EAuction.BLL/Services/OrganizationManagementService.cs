using EAuction.BLL.ViewModels;
using EAuction.Infrastructure;
using EAuction.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EAuction.Core.DataModels;
using EAuction.BLL.ExternalModels;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace EAuction.BLL.Sevices
{
    public class OrganizationManagementService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IdentityDbContext _identityDbContext;

        public void OpenOrganization(RegisterOrganizationViewModel model)
        {
            var geoLocationInfo = GeoLocationInfo.GetGeolocationInfo();

            if (model == null)
                throw new Exception($"{typeof(RegisterOrganizationViewModel).Name} is null");

            var checkOrganization = _applicationDbContext.Organizations
                .SingleOrDefault(p => p.IdentificationNumber == model.IdentificationNumber || p.FullName == model.FullName);
            if (checkOrganization != null)
                throw new Exception("Такая организация уже сущуствует в базе");

            var checkOrganizationType = _applicationDbContext.OrganizationTypes
                .SingleOrDefault(p => p.Name == model.OrganizationType);
            if (checkOrganizationType == null)
            {
                OrganizationType orgType = new OrganizationType()
                {
                    Id = Guid.NewGuid(),
                    Name = model.OrganizationType
                };
                _applicationDbContext.OrganizationTypes.Add(orgType);
                _applicationDbContext.SaveChanges();
                checkOrganizationType = orgType;
            }


            Organization organization = new Organization()
            {
                Id = Guid.NewGuid(),
                FullName = model.FullName,
                IdentificationNumber = model.IdentificationNumber,
                RegistrationDate = DateTime.Now,
                OrganizationTypeId = checkOrganizationType.Id
            };
            _applicationDbContext.Organizations.Add(organization);
            _applicationDbContext.SaveChanges();

            var checkEmployeeEmail = _applicationDbContext.Employees.Any(p => p.Email == model.CeoEmail);
            if (!checkEmployeeEmail)
            {
                var ceoPosition = _applicationDbContext.EmployeePositions.SingleOrDefault(p => p.Name == "CEO");
                if (ceoPosition==null)
                {
                    EmployeePosition pos = new EmployeePosition()
                    {
                        Id = Guid.NewGuid(),
                        Name = "CEO"
                    };
                    _applicationDbContext.EmployeePositions.Add(pos);
                    _applicationDbContext.SaveChanges();
                    ceoPosition = pos;
                }

                Employee employee = new Employee()
                {
                    Id = Guid.NewGuid(),
                    FirstName = model.CeoFirstName,
                    LastName = model.CeoLastName,
                    DoB = model.CeoDoB,
                    Email = model.CeoEmail,
                    EmployeePositionId = new Guid(ceoPosition.Id.ToString()),
                    OrganizationId=organization.Id
                };
                _applicationDbContext.Employees.Add(employee);
                _applicationDbContext.SaveChanges();

                ApplicationUser user = new ApplicationUser()
                {
                    Id = Guid.NewGuid(),
                    Email = model.CeoEmail,
                    IsActive = true,
                    FailedSignInCount = 0,
                    CreatedDate = DateTime.Now,
                    AssosiatedEmployeeId = employee.Id
                };
                _identityDbContext.ApplicationUsers.Add(user);
                _identityDbContext.SaveChanges();

                ApplicationUserPasswordHistory userPasswordHistory = new ApplicationUserPasswordHistory()
                {
                    Id = Guid.NewGuid(),
                    SetupDate = DateTime.Now,
                    Password = model.Password,
                    ApplicationUserId = user.Id
                };
                _identityDbContext.ApplicationUserPasswordHistories.Add(userPasswordHistory);
                _identityDbContext.SaveChanges();

                ApplicationUserSignInHistory userSignInHistory = new ApplicationUserSignInHistory()
                {
                    Id = Guid.NewGuid(),
                    SignInTime = DateTime.Now,
                    MachineIp = geoLocationInfo.ip,
                    IpToGeoCountry = geoLocationInfo.country_name,
                    IpToGeoCity = geoLocationInfo.city,
                    IpToGeoLatitude = geoLocationInfo.latitude,
                    IpToGeoLongitude = geoLocationInfo.longitude,
                    ApplicationUserId = user.Id
                };
                _identityDbContext.ApplicationUserSignInHistories.Add(userSignInHistory);
                _identityDbContext.SaveChanges();
            }                         
            
        }

        public void EditOrganizationInfo(OrganizationInfoViewModel model)
        {
            var organization = _applicationDbContext.Organizations.SingleOrDefault(p => p.Id.ToString() == model.OrganizationId);
            if (organization == null)
                throw new Exception($"Организации с id {model.OrganizationId} в базе нет");

            var organizationType = _applicationDbContext.OrganizationTypes
                .SingleOrDefault(p => p.Name == model.OrganizationType);
            if (organizationType==null)
            {
                OrganizationType orgType = new OrganizationType()
                {
                    Id = Guid.NewGuid(),
                    Name = model.OrganizationType
                };
                _applicationDbContext.OrganizationTypes.Add(orgType);
                _applicationDbContext.SaveChanges();
                organizationType = orgType;
            }

            organization.FullName = model.FullName;
            organization.IdentificationNumber = model.IdentificationNumber;
            organization.OrganizationTypeId = organizationType.Id;
            organization.Address = model.Address;
            organization.Email = model.Email;
            organization.Contacts = model.Contacts;
            organization.Site = model.Site;
            _applicationDbContext.SaveChanges();
            
            //if (model.UploadedFiles.Count != 0)
            //{
            //    foreach (HttpPostedFileBase i in model.UploadedFiles)
            //    {
            //        OrganizationFile file = new OrganizationFile();
            //        byte[] fileData = null;
                    
            //        using (var binaryReader = new BinaryReader(i.InputStream))
            //        {
            //            fileData = binaryReader.ReadBytes(i.ContentLength);
            //        }

            //        file.OrganizationId = organization.Id;
            //        file.FileName = i.FileName;
            //        file.Extension = i.ContentType;
            //        file.Content = fileData;
            //        file.CreatedAt = DateTime.Now;
            //        _applicationDbContext.OrganizationFiles.Add(file);
            //        _applicationDbContext.SaveChanges();
            //    }                
            //}
            
        }
        public OrganizationInfoViewModel GetOrganizationInfo(Guid organizationId)
        {
            var organization = _applicationDbContext.Organizations.Include("OrganizationRatings").SingleOrDefault(p => p.Id == organizationId);
            if (organization == null)
                throw new Exception($"Организации с таким id {organizationId} не существует");

            var organizationFiles = _applicationDbContext.OrganizationFiles.Where(p => p.OrganizationId == organizationId).ToList();            
            var averageScore = organization.OrganizationRatings.Average(p => p.Score);

            OrganizationInfoViewModel model = new OrganizationInfoViewModel()
            {
                OrganizationId = organization.Id.ToString(),
                FullName = organization.FullName,
                IdentificationNumber = organization.IdentificationNumber,
                OrganizationType = organization.OrganizationType.Name,
                OrganizationRating = averageScore,
                Address = organization.Address,
                Email = organization.Email,
                Contacts = organization.Contacts,
                Site = organization.Site,
                OrganizationFiles = organizationFiles
            };

            return model;
        }


        public List<OrganizationInfoViewModel> GetAllOrganizationsInfo()
        {
            var organizations = _applicationDbContext.Organizations.ToList();
            if (organizations.Count == 0)
                throw new Exception("В базе нет ни одной организации");

            List<OrganizationInfoViewModel> organizationInfos = new List<OrganizationInfoViewModel>();
            foreach (Organization item in organizations)
            {
                var model = GetOrganizationInfo(item.Id);
                organizationInfos.Add(model);
            }

            return organizationInfos;            
        }


        public void PutRatingScoreToOrganization(Guid organizationId, double score)
        {
            var organization = _applicationDbContext.Organizations.SingleOrDefault(p => p.Id == organizationId);
            if (organization == null)
                throw new Exception($"Организации с таким id {organizationId} не существует");

            OrganizationRating rating = new OrganizationRating()
            {
                Id = Guid.NewGuid(),
                Score = score,
                OrganizationId = organization.Id
            };
            _applicationDbContext.OrganizationRatings.Add(rating);
            _applicationDbContext.SaveChanges();
        }
        

        public void MakeTransaction(TransactionInfoViewModel model)
        {
            var organization = _applicationDbContext.Organizations
                .SingleOrDefault(p => p.FullName == model.OrganizationName || p.Id.ToString()==model.OrganizationId);
            if (organization == null)
                throw new Exception($"Организации с таким наименованием {model.OrganizationName} в базе не существует");
            var transactionType = model.TransactionTypeName == TransactionType.Deposit.ToString() ? TransactionType.Deposit : TransactionType.Withdraw;

            Transaction transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                TransactionType = transactionType,
                Sum = model.Sum,
                TransactionDate = DateTime.Now,
                Description = model.Description,
                OrganizationId = organization.Id
            };
            _applicationDbContext.Transactions.Add(transaction);
            _applicationDbContext.SaveChanges();
        }

        public static RegisterOrganizationViewModel GetOrganizationInfoForRegistration()
        {
            RegisterOrganizationViewModel model = new RegisterOrganizationViewModel();
            Console.WriteLine("Введите название организации :");
            model.FullName = Console.ReadLine();
            Console.WriteLine("Введите идентификационный номер организации :");
            model.IdentificationNumber = Console.ReadLine();
            Console.WriteLine("Введите тип организации :");
            model.OrganizationType = Console.ReadLine();
            Console.WriteLine("Введите имя руководителя организации :");
            model.CeoFirstName = Console.ReadLine();
            Console.WriteLine("Введите фамилию руководителя организации :");
            model.CeoLastName = Console.ReadLine();
            Console.WriteLine("Введите email организации :");
            model.CeoEmail = Console.ReadLine();
            Console.WriteLine("Введите дату рождения руководителя организации :");
            model.CeoDoB = DateTime.Parse(Console.ReadLine());
            Console.WriteLine("Введите пароль :");
            model.Password = Console.ReadLine();
            Console.WriteLine("Подтвердите пароль :");
            model.PasswordConfirm = Console.ReadLine();

            return model;
        }

        public Guid GetOrganizationInfoByUser(UserLogOnViewModel user)
        {
            var temp = _identityDbContext.ApplicationUsers.SingleOrDefault(s => s.Email == user.Email); //_applicationDbContext.Organizations.Include("Employees");

            var emp = _applicationDbContext.Employees.SingleOrDefault(s => s.Id == temp.AssosiatedEmployeeId);

            var model =_applicationDbContext.Organizations.SingleOrDefault(s => s.Id == emp.OrganizationId);
            return model.Id;
        }

        public OrganizationManagementService()
        {
            _applicationDbContext = new ApplicationDbContext();
            _identityDbContext = new IdentityDbContext();
        }
    }
}