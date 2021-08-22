using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using Carly.Authorization.Roles;
using Carly.Authorization.Users;
using Carly.MultiTenancy;
using Carly.Principals;
using Carly.AddOns;
using Carly.Vouchers;
using Carly.CustomerPrincipals;
using Carly.CustomerAddOns;
using Carly.Packages;
using Carly.Sales;
using Carly.LogoLinks;

namespace Carly.EntityFrameworkCore
{
    public class CarlyDbContext : AbpZeroDbContext<Tenant, Role, User, CarlyDbContext>
    {
        public DbSet<Principal> Principals { get; set; }
        public DbSet<AddOn> AddOns { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<CustomerPrincipal> CustomerPrincipals { get; set; }
        public DbSet<CustomerAddOn> CustomerAddOns { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<GeneratedVoucher> GeneratedVouchers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<LogoLink> LogoLinks { get; set; }

        /* Define a DbSet for each entity of the application */

        public CarlyDbContext(DbContextOptions<CarlyDbContext> options)
            : base(options)
        {
        }
    }
}
