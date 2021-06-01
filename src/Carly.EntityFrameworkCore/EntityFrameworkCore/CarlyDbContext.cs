using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using Carly.Authorization.Roles;
using Carly.Authorization.Users;
using Carly.MultiTenancy;
using Carly.Principals;
using Carly.AddOns;
using Carly.Vouchers;

namespace Carly.EntityFrameworkCore
{
    public class CarlyDbContext : AbpZeroDbContext<Tenant, Role, User, CarlyDbContext>
    {
        public DbSet<Principal> Principals { get; set; }
        public DbSet<AddOn> AddOns { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        /* Define a DbSet for each entity of the application */

        public CarlyDbContext(DbContextOptions<CarlyDbContext> options)
            : base(options)
        {
        }
    }
}
