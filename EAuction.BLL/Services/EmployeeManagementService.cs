using EAuction.BLL.ViewModels;
using EAuction.Core.DataModels;
using EAuction.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EAuction.BLL.Services
{
    public class EmployeeManagementService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IdentityDbContext _identityDbContext;

        public void CreateEmployeeByCeo(EmployeeInfoViewModel model, Guid ceoId)
        {
            var employee = _applicationDbContext.Employees.SingleOrDefault(p => p.Id == ceoId);
            if (employee==null)
                throw new Exception($"Работника с id {ceoId} в базе нет");

            var ceo = _applicationDbContext.EmployeePositions.SingleOrDefault(p => p.Name == "CEO");
            if (ceo==null)
            {
                EmployeePosition pos = new EmployeePosition()
                {
                    Id = Guid.NewGuid(),
                    Name = "CEO"
                };
                _applicationDbContext.EmployeePositions.Add(pos);
                _applicationDbContext.SaveChanges();
                ceo = pos;
            }
            if (employee.EmployeePositionId!=ceo.Id)
                throw new Exception($"Работник с id {ceoId} не может создавать аккаунты других сотрудников компании");

            var position = _applicationDbContext.EmployeePositions.SingleOrDefault(p => p.Name == model.PositionName);
            if (position == null)
            {
                EmployeePosition pos = new EmployeePosition()
                {
                    Id = Guid.NewGuid(),
                    Name = model.PositionName
                };
                _applicationDbContext.EmployeePositions.Add(pos);
                _applicationDbContext.SaveChanges();
                position = pos;
            }            

            Employee newEmployee = new Employee()
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                DoB = model.DoB,
                Email = model.Email,
                EmployeePositionId= position.Id,
                OrganizationId= employee.OrganizationId
            };
            _applicationDbContext.Employees.Add(newEmployee);
            _applicationDbContext.SaveChanges();

            ApplicationUser user = new ApplicationUser()
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                IsActive = true,
                FailedSignInCount = 0,
                CreatedDate = DateTime.Now,
                AssosiatedEmployeeId = newEmployee.Id
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
        }


        public void EditEmployeeInfo(EmployeeInfoViewModel model)
        {
            var employee = _applicationDbContext.Employees.SingleOrDefault(p => p.Id.ToString() == model.EmployeeId);
            if (employee == null)
                throw new Exception($"Работника с id {model.EmployeeId} в базе нет");

            var position = _applicationDbContext.EmployeePositions.SingleOrDefault(p => p.Name == model.PositionName);
            if (position==null)
            {
                EmployeePosition pos = new EmployeePosition()
                {
                    Id = Guid.NewGuid(),
                    Name = model.PositionName
                };
                _applicationDbContext.EmployeePositions.Add(pos);
                _applicationDbContext.SaveChanges();
                position = pos;
            }

            var oldEmail = employee.Email;

            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            employee.DoB = model.DoB;
            employee.Email = model.Email;
            employee.EmployeePositionId = position.Id;
            _applicationDbContext.SaveChanges();

            var user = _identityDbContext.ApplicationUsers.Include("ApplicationUserPasswordHistories")
                .SingleOrDefault(p => p.AssosiatedEmployeeId.ToString() == model.EmployeeId && p.IsActive == true);
            var userPasswordHistory = user.ApplicationUserPasswordHistories
                .SingleOrDefault(p => p.InvalidatedDate == null);

            if (oldEmail!=model.Email)
            {
                user.IsActive = false;
                userPasswordHistory.InvalidatedDate = DateTime.Now;
                _identityDbContext.SaveChanges();

                ApplicationUser newUser = new ApplicationUser()
                {
                    Id = Guid.NewGuid(),
                    Email = model.Email,
                    IsActive = true,
                    FailedSignInCount = 0,
                    CreatedDate = DateTime.Now,
                    AssosiatedEmployeeId = employee.Id
                };
                _identityDbContext.ApplicationUsers.Add(newUser);
                _identityDbContext.SaveChanges();                

                ApplicationUserPasswordHistory newUserPasswordHistory = new ApplicationUserPasswordHistory()
                {
                    Id = Guid.NewGuid(),
                    SetupDate = DateTime.Now,
                    Password = model.Password,
                    ApplicationUserId = newUser.Id
                };
                _identityDbContext.ApplicationUserPasswordHistories.Add(newUserPasswordHistory);
                _identityDbContext.SaveChanges();
            }
            else
            {
                if (userPasswordHistory.Password!=model.Password)
                {
                    userPasswordHistory.InvalidatedDate = DateTime.Now;
                    _identityDbContext.SaveChanges();

                    ApplicationUserPasswordHistory userNewPasswordHistory = new ApplicationUserPasswordHistory()
                    {
                        Id = Guid.NewGuid(),
                        SetupDate = DateTime.Now,
                        Password = model.Password,
                        ApplicationUserId = user.Id
                    };
                    _identityDbContext.ApplicationUserPasswordHistories.Add(userNewPasswordHistory);
                    _identityDbContext.SaveChanges();
                }
            }
            
        }


        public EmployeeInfoViewModel GetEmployeeInfo(Guid employeeId)
        {
            var employee = _applicationDbContext.Employees.SingleOrDefault(p => p.Id == employeeId);
            if (employee == null)
                throw new Exception($"Работника с id {employeeId} в базе нет");

            EmployeeInfoViewModel model = new EmployeeInfoViewModel()
            {
                EmployeeId = employee.Id.ToString(),
                PositionName = employee.EmployeePosition.Name,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                DoB = employee.DoB,
                Email = employee.Email
            };

            return model;
        }

        public List<EmployeeInfoViewModel> GetEmployeesInfoByOrganization(Guid organizationId)
        {
            var organization = _applicationDbContext.Organizations.Include("Employees").SingleOrDefault(p => p.Id == organizationId);
            if (organization == null)
                throw new Exception($"Организации с id {organizationId} в базе не существует");

            var employees = organization.Employees.ToList();
            if (employees.Count==0)
                throw new Exception($"В организации {organization.FullName} нет ни одного сотрудника");

            List<EmployeeInfoViewModel> employeeInfos = new List<EmployeeInfoViewModel>();
            foreach (Employee item in employees)
            {
                var model = GetEmployeeInfo(item.Id);
                employeeInfos.Add(model);
            }

            return employeeInfos;
        }



        public EmployeeManagementService()
        {
            _applicationDbContext = new ApplicationDbContext();
            _identityDbContext = new IdentityDbContext();
        }
    }
}