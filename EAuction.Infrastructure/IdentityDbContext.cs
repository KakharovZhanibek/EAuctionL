using EAuction.Core.DataModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAuction.Infrastructure
{
    public class IdentityDbContext:DbContext
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationUserPasswordHistory> ApplicationUserPasswordHistories { get; set; }
        public DbSet<ApplicationUserSignInHistory> ApplicationUserSignInHistories { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().HasMany(p => p.ApplicationUserPasswordHistories).WithRequired(p => p.ApplicationUser).HasForeignKey(p => p.ApplicationUserId);
            modelBuilder.Entity<ApplicationUser>().HasMany(p => p.ApplicationUserSignInHistories).WithRequired(p => p.ApplicationUser).HasForeignKey(p => p.ApplicationUserId);
            base.OnModelCreating(modelBuilder);
        }

        public IdentityDbContext():base("EIdentityConnectionString")
        {

        }
    }
}
