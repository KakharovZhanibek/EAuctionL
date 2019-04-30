using EAuction.BLL.ViewModels;
using EAuction.Core.DataModels;
using EAuction.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace EAuction.BLL.Services
{
    public class AuctionManagementService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public void OpenAuction(CreateAuctionViewModel model, Guid organizationId)
        {
            if (model == null)
                throw new Exception($"{typeof(CreateAuctionViewModel).Name} is null");

            int maximumAllowedActiveAuctions = 3;

            var auctionsCheck = _applicationDbContext.Auctions
                .Where(p =>p.OrganizationId==organizationId && p.Status == AuctionStatus.Active)
                .Count() < maximumAllowedActiveAuctions;

            var categoryCheck = _applicationDbContext.AuctionTypes
                .SingleOrDefault(p => p.Name == model.AuctionType);
            if (categoryCheck == null)
            {
                AuctionType auctionType = new AuctionType()
                {
                    Id = Guid.NewGuid(),
                    Name = model.AuctionType
                };
                _applicationDbContext.AuctionTypes.Add(auctionType);
                _applicationDbContext.SaveChanges();
                categoryCheck = auctionType;
            }

            if (!auctionsCheck)
                throw new Exception("Превышено максимальное количество активных аукционов!");


            Auction auction = new Auction()
            {
                Id = Guid.NewGuid(),
                Description = model.Description,
                ShippingAddress = model.ShippingAddress,
                ShippingConditions = model.ShippingConditions,
                MinRatingForParticipant=model.RatingForParticipant,
                StartPrice = model.StartPrice,
                PriceStep = model.PriceStep,
                MinPrice = model.MinPrice,
                StartDate = model.StartDate,
                FinishDate = model.FinishDate,
                Status = AuctionStatus.Active,
                AuctionTypeId = categoryCheck.Id,
                OrganizationId = organizationId
            };
            _applicationDbContext.Auctions.Add(auction);
            _applicationDbContext.SaveChanges();
        }

        public void MakeBidToAuction(MakeBidViewModel model, decimal bidCost)
        {
            var bidExists = _applicationDbContext.Bids
                .Any(p => p.Price == model.Price &&
                p.AuctionId.ToString() == model.AuctionId &&
                p.Description == model.Description &&
                p.OrganizationId.ToString() == model.OrganizationId);

            if (bidExists)
                throw new Exception("Такая ставка уже существует");

            var organization = _applicationDbContext.Organizations.Include("Transactions")
                .SingleOrDefault(p => p.Id.ToString() == model.OrganizationId);

            if (organization == null)
                throw new Exception("Такой организации в БД нет");

            var auction = _applicationDbContext.Auctions
                .SingleOrDefault(p => p.Id.ToString() == model.AuctionId);
            if (auction.OrganizationId.ToString() == model.OrganizationId)
                throw new Exception("Создатель аукциона не может ставить ставки");

            var organizationTransactions = organization.Transactions.ToList();
            if (organizationTransactions.Count == 0)
                throw new Exception($"У организации {organization.FullName} нулевой баланс");

            var organizationBalance = organizationTransactions.Where(p => p.TransactionType == TransactionType.Deposit).Sum(p => p.Sum) -
                organizationTransactions.Where(p => p.TransactionType == TransactionType.Withdraw).Sum(p => p.Sum);
            if (organizationBalance < bidCost)
                throw new Exception($"У организации {organization.FullName} не хватает средств на балансе для оплаты стоимости участия в аукционе");

            var inValidPriceRange = _applicationDbContext.Auctions
                .Where(p => p.Id.ToString() == model.AuctionId &&
                p.MinPrice < model.Price &&
                p.StartPrice > model.Price).ToList();

            var inStepRange = inValidPriceRange
                .Any(p => (p.StartPrice - model.Price) % p.PriceStep == 0);

            if (!inStepRange)
                throw new Exception("Invalid bid according price step");

            var activeBidStatus = _applicationDbContext.BidStatuses.SingleOrDefault(p => p.StatusName == "Active");
            if (activeBidStatus == null)
            {
                BidStatus status = new BidStatus()
                {
                    Id = Guid.NewGuid(),
                    StatusName = "Active"
                };
                _applicationDbContext.BidStatuses.Add(status);
                _applicationDbContext.SaveChanges();
                activeBidStatus = status;
            }

            //делаем ставку и списываем деньги за участие
            Bid bid = new Bid()
            {
                Id = Guid.NewGuid(),
                Price = model.Price,
                IsWin = false,
                Description = model.Description,
                AuctionId = new Guid(model.AuctionId),
                OrganizationId = new Guid(model.OrganizationId),
                CreatedDate = DateTime.Now,
                BidStatusId = activeBidStatus.Id
            };
            _applicationDbContext.Bids.Add(bid);
            _applicationDbContext.SaveChanges();

            Transaction transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                Sum = bidCost,
                TransactionType = TransactionType.Withdraw,
                TransactionDate = DateTime.Now,
                OrganizationId = new Guid(model.OrganizationId),
                Description = $"Withdraw participation cost for auction {model.AuctionId}"
            };
            _applicationDbContext.Transactions.Add(transaction);
            _applicationDbContext.SaveChanges();

        }


        public void DeleteBidFromAuction(Guid bidId)
        {
            var bidExists = _applicationDbContext.Bids.SingleOrDefault(p => p.Id == bidId);
            if (bidExists==null)
                throw new Exception("Такой ставки не существует!");

            var revokeBidStatus = _applicationDbContext.BidStatuses.SingleOrDefault(p => p.StatusName == "Revoke");
            if (revokeBidStatus == null)
            {
                BidStatus status = new BidStatus()
                {
                    Id = Guid.NewGuid(),
                    StatusName = "Revoke"
                };
                _applicationDbContext.BidStatuses.Add(status);
                _applicationDbContext.SaveChanges();
                revokeBidStatus = status;
            }

            var auctionFinishDate = _applicationDbContext.Auctions.SingleOrDefault(p => p.Id == bidExists.AuctionId);
            if ((auctionFinishDate.FinishDate - DateTime.Now).Days < 1)
                throw new Exception("Ставку нельзя удалить! До завершение аукциона осталось меньше 24 часов.");

            bidExists.BidStatusId = revokeBidStatus.Id;            
            _applicationDbContext.SaveChanges();
        }


        public void WinnerInAuction(BidInfoViewModel model) 
        {
            var auction = _applicationDbContext.Auctions.SingleOrDefault(p => p.Id.ToString() == model.AuctionId);
            if (auction == null)
                throw new Exception($"Аукциона с id {model.AuctionId} не существует");

            var organization = _applicationDbContext.Organizations.Include("Transactions").Include("OrganizationRatings")
                .SingleOrDefault(p => p.Id.ToString() == model.OrganizationId);
            if (organization == null)
                throw new Exception($"Организации с id {model.OrganizationId} не существует");

            var IsAuctionCreator = _applicationDbContext.Auctions
                .Any(p => p.Id.ToString() == model.AuctionId && p.OrganizationId.ToString() == model.OrganizationId);
            if (IsAuctionCreator)
                throw new Exception($"Организация-создатель аукциона {model.AuctionId} не может быть победителем данного аукциона");
            

            var bid = _applicationDbContext.Bids.SingleOrDefault(p => p.Id.ToString() == model.BidId);
            if (bid == null)
                throw new Exception("Такой ставки не существует");

            bid.IsWin = true;
            _applicationDbContext.SaveChanges();

            Transaction transaction = new Transaction()
            {
                Id = Guid.NewGuid(),
                TransactionType = TransactionType.Withdraw,
                Sum = model.Price,
                TransactionDate = DateTime.Now,
                OrganizationId = new Guid(model.OrganizationId),
                Description = $"Withdraw bid price for auction {model.AuctionId}"
            };
            _applicationDbContext.Transactions.Add(transaction);
            _applicationDbContext.SaveChanges();
        }


        public void PutActualFinishDateToAuction(Guid auctionId, DateTime finishDate)
        {
            var auction = _applicationDbContext.Auctions.SingleOrDefault(p => p.Id == auctionId);
            if (auction == null)
                throw new Exception($"Аукциона с таким id {auctionId} не существует");

            auction.FinishDateAtActual = finishDate;
            _applicationDbContext.SaveChanges();
        }


        public AuctionInfoViewModel GetAuctionInfo(string auctionId)
        {            
            var auction = _applicationDbContext.Auctions.Include("Organization").SingleOrDefault(p => p.Id.ToString() == auctionId);
            if (auction == null)
                throw new Exception($"Аукциона с таким id {auctionId} не существует");

            var auctionFiles = _applicationDbContext.AuctionFiles.Where(p => p.AuctionId.ToString() == auctionId).ToList();
            //if (auctionFiles.Count == 0)
            //    throw new Exception($"У аукциона {auctionId} нет прикрепленных документов");

            
            var auctionType = _applicationDbContext.AuctionTypes.SingleOrDefault(p => p.Id == auction.AuctionTypeId);
            
            var organization = _applicationDbContext.Organizations.SingleOrDefault(p => p.Id == auction.OrganizationId);
            
            AuctionInfoViewModel model = new AuctionInfoViewModel()
            {
                AuctionId = auctionId.ToString(),
                Status = auction.Status.ToString(),
                AuctionType = auctionType.Name,
                OrganizationName = organization.FullName,
                MinRatingForParticipant=auction.MinRatingForParticipant.ToString(),
                Description=auction.Description,
                ShippingAddress = auction.ShippingAddress,
                ShippingConditions = auction.ShippingConditions,
                StartPrice = auction.StartPrice,
                PriceStep = auction.PriceStep,
                MinPrice = auction.MinPrice,
                StartDate = auction.StartDate,
                FinishDate = auction.FinishDate,
                FinishDateAtActual = auction.FinishDateAtActual,
                //AuctionFiles = auctionFiles
            };

            return model;
        }

        public List<BidInfoViewModel> GetAllBidsForAuction(Guid auctionId)
        {
            var auction = _applicationDbContext.Auctions.Include("Organizations").Include("Bids").SingleOrDefault(p => p.Id == auctionId);
            if (auction == null)
                throw new Exception($"Аукциона с таким id {auctionId} не существует");

            var activeBidStatus = _applicationDbContext.BidStatuses.SingleOrDefault(p => p.StatusName == "Active").Id;
            if (_applicationDbContext.BidStatuses.SingleOrDefault(p => p.StatusName == "Active").Id == null)
            {
                BidStatus status = new BidStatus()
                {
                    Id = Guid.NewGuid(),
                    StatusName = "Active"
                };
                activeBidStatus = status.Id;
            }

            var bidForAuction = auction.Bids.Where(p=>p.BidStatusId== activeBidStatus).ToList();
            if (bidForAuction.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет активных ставок");

            List<BidInfoViewModel> bids = new List<BidInfoViewModel>();
            foreach (Bid item in bidForAuction)
            {
                BidInfoViewModel bid = new BidInfoViewModel()
                {
                    BidId = item.Id.ToString(),
                    AuctionId = auctionId.ToString(),
                    AuctionType = item.Auction.AuctionType.ToString(),
                    AuctionDescription = item.Auction.Description,
                    BidStatus = item.BidStatus.StatusName,
                    OrganizationId = item.Organization.Id.ToString(),
                    OrganizationName = item.Organization.FullName,
                    Price = item.Price,
                    CreatedDate = item.CreatedDate,
                    BidDescription = item.Description
                };
                bids.Add(bid);
            }
            return bids;
        }

        public List<BidInfoViewModel> GetBidsForAuctionWithFitOrganizationRating(Guid auctionId)
        {
            var auction = _applicationDbContext.Auctions.Include("Organizations").Include("Bids").SingleOrDefault(p => p.Id == auctionId);
            if (auction == null)
                throw new Exception($"Аукциона с таким id {auctionId} не существует");

            var activeBidStatus = _applicationDbContext.BidStatuses.SingleOrDefault(p => p.StatusName == "Active").Id;
            if (activeBidStatus == null)
            {
                BidStatus status = new BidStatus()
                {
                    Id = Guid.NewGuid(),
                    StatusName = "Active"
                };
                activeBidStatus = status.Id;
            }

            var allBids = auction.Bids.Where(p => p.BidStatusId == activeBidStatus).ToList();
            if (allBids.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет активных ставок");

            List<Bid> bidsWithFitRating = new List<Bid>();
            foreach (Bid item in allBids)
            {
                var org = item.OrganizationId;
                var avg = _applicationDbContext.OrganizationRatings.Where(p => p.OrganizationId == org).Average(p => p.Score);
                if (avg >= auction.MinRatingForParticipant)
                    bidsWithFitRating.Add(item);
            }
            if (bidsWithFitRating.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет ставок от организаций с нужным рейтингом");

            List<BidInfoViewModel> bids = new List<BidInfoViewModel>();
            foreach (Bid item in bidsWithFitRating)
            {
                BidInfoViewModel bid = new BidInfoViewModel()
                {
                    BidId = item.Id.ToString(),
                    AuctionId = auctionId.ToString(),
                    AuctionType = item.Auction.AuctionType.ToString(),
                    AuctionDescription = item.Auction.Description,
                    BidStatus = item.BidStatus.StatusName,
                    OrganizationId = item.Organization.Id.ToString(),
                    OrganizationName = item.Organization.FullName,
                    Price = item.Price,
                    CreatedDate = item.CreatedDate,
                    BidDescription = item.Description
                };
                bids.Add(bid);
            }
            return bids;
        }

        public List<BidInfoViewModel> GetBidsForAuctionWithFitOrganizationBalance(Guid auctionId)
        {
            var auction = _applicationDbContext.Auctions.Include("Organizations").Include("Bids").SingleOrDefault(p => p.Id == auctionId);
            if (auction == null)
                throw new Exception($"Аукциона с таким id {auctionId} не существует");

            var activeBidStatus = _applicationDbContext.BidStatuses.SingleOrDefault(p => p.StatusName == "Active").Id;
            if (activeBidStatus == null)
            {
                BidStatus status = new BidStatus()
                {
                    Id = Guid.NewGuid(),
                    StatusName = "Active"
                };
                activeBidStatus = status.Id;
            }

            var allBids = auction.Bids.Where(p=>p.BidStatusId== activeBidStatus).ToList();
            if (allBids.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет активных ставок");

            List<Bid> bidsWithFitBalance = new List<Bid>();
            foreach (Bid item in allBids)
            {
                var org = item.OrganizationId;
                var sumDeposits = _applicationDbContext.Transactions
                    .Where(p => p.OrganizationId == org && p.TransactionType == TransactionType.Deposit).Sum(p => p.Sum);
                var sumWithdraws = _applicationDbContext.Transactions
                    .Where(p => p.OrganizationId == org && p.TransactionType == TransactionType.Withdraw).Sum(p => p.Sum);
                if (sumDeposits- sumWithdraws > item.Price)
                    bidsWithFitBalance.Add(item);
            }
            if (bidsWithFitBalance.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет ставок от организаций с достаточным балансом");

            List<BidInfoViewModel> bids = new List<BidInfoViewModel>();
            foreach (Bid item in bidsWithFitBalance)
            {
                BidInfoViewModel bid = new BidInfoViewModel()
                {
                    BidId = item.Id.ToString(),
                    AuctionId = auctionId.ToString(),
                    AuctionType = item.Auction.AuctionType.ToString(),
                    AuctionDescription = item.Auction.Description,
                    BidStatus = item.BidStatus.StatusName,
                    OrganizationId = item.Organization.Id.ToString(),
                    OrganizationName = item.Organization.FullName,
                    Price = item.Price,
                    CreatedDate = item.CreatedDate,
                    BidDescription = item.Description
                };
                bids.Add(bid);
            }
            return bids;
        }

        public List<BidInfoViewModel> GetBidsForAuctionWithFitOrganizationRatingAndBalance(Guid auctionId)
        {
            var auction = _applicationDbContext.Auctions.Include("Organizations").Include("Bids").SingleOrDefault(p => p.Id == auctionId);
            if (auction == null)
                throw new Exception($"Аукциона с таким id {auctionId} не существует");

            var activeBidStatus = _applicationDbContext.BidStatuses.SingleOrDefault(p => p.StatusName == "Active").Id;
            if (activeBidStatus == null)
            {
                BidStatus status = new BidStatus()
                {
                    Id = Guid.NewGuid(),
                    StatusName = "Active"
                };
                activeBidStatus = status.Id;
            }

            var allBids = auction.Bids.Where(p=>p.BidStatusId==activeBidStatus).ToList();
            if (allBids.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет активных ставок");

            List<Bid> bidsWithFitRating = new List<Bid>();
            foreach (Bid item in allBids)
            {
                var org = item.OrganizationId;
                var avg = _applicationDbContext.OrganizationRatings.Where(p => p.OrganizationId == org).Average(p => p.Score);
                if (avg >= auction.MinRatingForParticipant)
                    bidsWithFitRating.Add(item);
            }
            if (bidsWithFitRating.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет ставок от организаций с нужным рейтингом");

            List<Bid> bidsWithFitRatingBalance = new List<Bid>();
            foreach (Bid item in bidsWithFitRating)
            {
                var org = item.OrganizationId;
                var sumDeposits = _applicationDbContext.Transactions
                    .Where(p => p.OrganizationId == org && p.TransactionType == TransactionType.Deposit).Sum(p => p.Sum);
                var sumWithdraws = _applicationDbContext.Transactions
                    .Where(p => p.OrganizationId == org && p.TransactionType == TransactionType.Withdraw).Sum(p => p.Sum);
                if (sumDeposits - sumWithdraws > item.Price)
                    bidsWithFitRatingBalance.Add(item);
            }
            if (bidsWithFitRatingBalance.Count == 0)
                throw new Exception($"По аукциону {auctionId} нет ставок от организаций с достаточным балансом");

            List<BidInfoViewModel> bids = new List<BidInfoViewModel>();
            foreach (Bid item in bidsWithFitRatingBalance)
            {
                BidInfoViewModel bid = new BidInfoViewModel()
                {
                    BidId = item.Id.ToString(),
                    AuctionId = auctionId.ToString(),
                    AuctionType = item.Auction.AuctionType.ToString(),
                    AuctionDescription = item.Auction.Description,
                    BidStatus = item.BidStatus.StatusName,
                    OrganizationId = item.Organization.Id.ToString(),
                    OrganizationName = item.Organization.FullName,
                    Price = item.Price,
                    CreatedDate = item.CreatedDate,
                    BidDescription = item.Description
                };
                bids.Add(bid);
            }
            return bids;
        }

        public List<AuctionInfoViewModel> GetAllActiveAuctions()
        {
            var auctions = _applicationDbContext.Auctions.ToList();
            if (auctions.Count == 0)
                throw new Exception("Активных аукционов на данный момент в базе нет");

            List<AuctionInfoViewModel> auctionModels = new List<AuctionInfoViewModel>();
            foreach (Auction item in auctions)
            {
                AuctionInfoViewModel model = new AuctionInfoViewModel();
                model.AuctionId = item.Id.ToString();
                model.Status = item.Status.ToString();

                var auctionType = _applicationDbContext.AuctionTypes.SingleOrDefault(p => p.Id == item.AuctionTypeId);
                model.AuctionType = auctionType.Name;

                var organization = _applicationDbContext.Organizations.SingleOrDefault(p => p.Id == item.OrganizationId);
                model.OrganizationName = organization.FullName;

                model.MinRatingForParticipant = item.MinRatingForParticipant.ToString();
                model.Description = item.Description;
                model.ShippingAddress = item.ShippingAddress;
                model.ShippingConditions = item.ShippingConditions;
                model.StartPrice = item.StartPrice;
                model.PriceStep = item.PriceStep;
                model.MinPrice = item.MinPrice;
                model.StartDate = item.StartDate;
                model.FinishDate = item.FinishDate;
                //model.AuctionFiles = item.AuctionFiles.ToList();
                
                auctionModels.Add(model);
            }

            return auctionModels;
        }

        public List<AuctionInfoViewModel> GetAuctionsByOrganizationId(Guid organizationId)
        {
            var organization = _applicationDbContext.Organizations.SingleOrDefault(p => p.Id == organizationId);
            if (organization == null)
                throw new Exception($"Организации с таким id {organizationId} не существует");

            var auctions = _applicationDbContext.Auctions.Where(p => p.OrganizationId != organizationId).ToList();
            if (auctions.Count == 0)
                throw new Exception($"Аукционов, созданных организацией с id {organizationId} на данный момент в базе нет");

            List<AuctionInfoViewModel> auctionModels = new List<AuctionInfoViewModel>();
            foreach (Auction item in auctions)
            {
                AuctionInfoViewModel model = new AuctionInfoViewModel()
                {
                    AuctionId = item.Id.ToString(),
                    Status = item.Status.ToString(),
                    AuctionType = item.AuctionType.Name,
                    OrganizationName = item.Organization.FullName,
                    ShippingAddress = item.ShippingAddress,
                    ShippingConditions = item.ShippingConditions,
                    StartPrice = item.StartPrice,
                    PriceStep = item.PriceStep,
                    MinPrice = item.MinPrice,
                    StartDate = item.StartDate,
                    FinishDate = item.FinishDate,
                    AuctionFiles = item.AuctionFiles.ToList()
                };
                auctionModels.Add(model);
            }

            return auctionModels;
        }

        public AuctionManagementService()
        {
            _applicationDbContext = new ApplicationDbContext();
        }
    }
}