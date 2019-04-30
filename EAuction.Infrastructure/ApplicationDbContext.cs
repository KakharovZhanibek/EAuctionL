using EAuction.Core.DataModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Infrastructure
{
    public class ApplicationDbContext:DbContext
    {
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<AuctionFile> AuctionFiles { get; set; }
        public DbSet<AuctionType> AuctionTypes { get; set; }        
        public DbSet<Bid> Bids { get; set; }
        public DbSet<BidStatus> BidStatuses { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationFile> OrganizationFiles { get; set; }
        public DbSet<OrganizationType> OrganizationTypes { get; set; }
        public DbSet<OrganizationRating> OrganizationRatings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuctionType>().HasMany(p => p.Auctions).WithRequired(p => p.AuctionType).HasForeignKey(p => p.AuctionTypeId);
            modelBuilder.Entity<Auction>().HasMany(p => p.AuctionFiles).WithRequired(p => p.Auction).HasForeignKey(p => p.AuctionId);
            modelBuilder.Entity<Auction>().HasMany(p => p.Bids).WithRequired(p => p.Auction).HasForeignKey(p => p.AuctionId);
            modelBuilder.Entity<EmployeePosition>().HasMany(p => p.Employees).WithRequired(p => p.EmployeePosition).HasForeignKey(p => p.EmployeePositionId);
            modelBuilder.Entity<OrganizationType>().HasMany(p => p.Organizations).WithRequired(p => p.OrganizationType).HasForeignKey(p => p.OrganizationTypeId);
            modelBuilder.Entity<Organization>().HasMany(p => p.Auctions).WithRequired(p => p.Organization).HasForeignKey(p => p.OrganizationId);
            modelBuilder.Entity<Organization>().HasMany(p => p.OrganizationFiles).WithRequired(p => p.Organization).HasForeignKey(p => p.OrganizationId);
            modelBuilder.Entity<Organization>().HasMany(p => p.Employees).WithRequired(p => p.Organization).HasForeignKey(p => p.OrganizationId);
            modelBuilder.Entity<Organization>().HasMany(p => p.Bids).WithRequired(p => p.Organization).HasForeignKey(p => p.OrganizationId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Organization>().HasMany(p => p.Transactions).WithRequired(p => p.Organization).HasForeignKey(p => p.OrganizationId);
            modelBuilder.Entity<Organization>().HasMany(p => p.OrganizationRatings).WithRequired(p => p.Organization).HasForeignKey(p => p.OrganizationId);
            modelBuilder.Entity<BidStatus>().HasMany(p => p.Bids).WithRequired(p => p.BidStatus).HasForeignKey(p => p.BidStatusId);            
            base.OnModelCreating(modelBuilder);
        }

        public ApplicationDbContext():base("EAuctionConnectionString")
        {

        }

    }
}
